/**
 * HutechNovel TTS Engine v2
 * Engine: Google Translate TTS (unofficial, free, no API key)
 * Hỗ trợ: Mọi ngôn ngữ Google Translate (vi, zh-CN, zh-TW, en, ja, ko...)
 * Cơ chế: Tách đoạn văn dài thành chunks ≤ 180 ký tự, nối tiếp nhau qua Audio element.
 */

const TTSEngine = (() => {
    // ── Constants ────────────────────────────────────────────────────────────
    const GTTS_BASE = 'https://translate.google.com/translate_tts';
    const MAX_CHUNK_LEN = 180; // Google TTS giới hạn ~200 ký tự mỗi request

    // ── State ─────────────────────────────────────────────────────────────────
    let _blocks = [];        // Array<{text: string, chunks: string[]}>
    let _blockIdx = 0;       // Paragraph đang đọc
    let _chunkIdx = 0;       // Sub-chunk trong paragraph đang đọc
    let _audio = null;       // Audio element hiện tại
    let _isPlaying = false;
    let _isTransitioning = false; // Ngăn chặn sự cố gọi đúp khi error & catch cùng kích hoạt
    let _lang = 'vi';
    let _rate = 1.0;         // Ánh xạ vào audio.playbackRate

    // ── Callbacks ─────────────────────────────────────────────────────────────
    let _onBlockStart  = null;  // (blockIndex: number) => void
    let _onBlockEnd    = null;  // (blockIndex: number) => void
    let _onChapterEnd  = null;  // () => void
    let _onStop        = null;  // () => void
    let _onStateChange = null;  // (isPlaying: boolean) => void

    // ── Text Chunker ──────────────────────────────────────────────────────────
    /**
     * Tách đoạn văn dài thành mảng chunks ≤ maxLen ký tự.
     * Ưu tiên cắt tại dấu câu (。！？…,.，；;) để âm thanh tự nhiên hơn.
     */
    function _splitChunks(text, maxLen = MAX_CHUNK_LEN) {
        if (text.length <= maxLen) return [text];

        const result = [];
        // Tách theo dấu câu nhưng giữ lại dấu câu
        const sentenceRe = /([^.!?。！？…,，；;]+[.!?。！？…,，；;]*)/g;
        const parts = text.match(sentenceRe) || [text];

        let current = '';
        for (const part of parts) {
            if ((current + part).length <= maxLen) {
                current += part;
            } else {
                if (current.trim()) result.push(current.trim());
                if (part.length > maxLen) {
                    // Hard-split nếu 1 câu quá dài
                    for (let i = 0; i < part.length; i += maxLen) {
                        result.push(part.slice(i, i + maxLen).trim());
                    }
                    current = '';
                } else {
                    current = part;
                }
            }
        }
        if (current.trim()) result.push(current.trim());
        return result.filter(c => c.length > 0);
    }

    // ── Audio URL Builder ─────────────────────────────────────────────────────
    /**
     * Dùng backend proxy thay vì gọi Google TTS trực tiếp.
     * Lý do: Trình duyệt bị Google block (CORS/Referrer). Server gọi thay thì OK.
     * Route: GET /api/TTS/Speak?text=...&lang=...
     */
    function _buildUrl(text, lang) {
        return `/api/TTS/Speak?text=${encodeURIComponent(text)}&lang=${encodeURIComponent(lang)}`;
    }

    // ── Core Playback ─────────────────────────────────────────────────────────
    function _stopAudio() {
        if (_audio) {
            _audio.pause();
            _audio.removeAttribute('src');
            _audio.load();
        }
    }

    function _initAudioElement() {
        if (!_audio) {
            _audio = new Audio();
            
            _audio.addEventListener('ended', () => {
                if (!_isPlaying || _isTransitioning) return;
                _advance();
            });

            _audio.addEventListener('error', (e) => {
                if (!_isPlaying || _isTransitioning) return;
                console.warn(`[TTSEngine] Lỗi tải audio, bỏ qua chunk này.`, e);
                _advance();
            });
        }
        _audio.playbackRate = _rate;
    }

    function _advance() {
        _isTransitioning = true;
        _chunkIdx++;
        setTimeout(() => {
            _isTransitioning = false;
            _playCurrentChunk();
        }, 50);
    }

    function _playCurrentChunk() {
        if (_isTransitioning) return;

        // Hết chương
        if (_blockIdx >= _blocks.length) {
            _isPlaying = false;
            if (_onStateChange) _onStateChange(false);
            if (_onChapterEnd) _onChapterEnd();
            return;
        }

        const block = _blocks[_blockIdx];

        // Khi bắt đầu đoạn mới
        if (_chunkIdx === 0 && _onBlockStart) {
            _onBlockStart(_blockIdx);
        }

        // Hết chunks trong block này → chuyển sang block tiếp theo
        if (_chunkIdx >= block.chunks.length) {
            if (_onBlockEnd) _onBlockEnd(_blockIdx);
            _blockIdx++;
            _chunkIdx = 0;
            _playCurrentChunk();
            return;
        }

        const chunkText = block.chunks[_chunkIdx];
        
        // Bỏ qua nếu text trống
        if (!chunkText || !chunkText.trim()) {
            _advance();
            return;
        }

        let actualLang = _lang;
        if (_lang === 'auto') {
            // Nhận diện theo mức độ ưu tiên
            if (/[\u4E00-\u9FFF]/.test(chunkText)) {
                // Tiếng Trung (hoặc Nhật dùng Kanji). Mặc định zh-CN.
                actualLang = 'zh-CN';
            } else if (/[\u3040-\u30FF]/.test(chunkText)) {
                // Hiragana/Katakana thuần -> Tiếng Nhật
                actualLang = 'ja';
            } else if (/[àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ]/i.test(chunkText)) {
                // Chứa dấu Tiếng Việt -> Tiếng Việt
                actualLang = 'vi';
            } else if (/[a-zA-Z]/.test(chunkText)) {
                // Thuần chữ cái latin không dấu -> Tiếng Anh
                actualLang = 'en';
            } else {
                // Ký tự đặc biệt hoặc số -> fallback về tiếng Việt
                actualLang = 'vi';
            }
        }

        const url = _buildUrl(chunkText, actualLang);

        _initAudioElement();
        _audio.src = url;
        _audio.load();
        _audio.playbackRate = _rate;
        
        const playPromise = _audio.play();
        if (playPromise !== undefined) {
            playPromise.catch(err => {
                console.warn('[TTSEngine] Không thể phát audio:', err);
                if (_isPlaying && !_isTransitioning) {
                    _advance();
                }
            });
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────
    return {
        /**
         * Khởi tạo engine với danh sách các đoạn văn.
         * @param {string[]} textBlocks - Mảng các đoạn văn
         * @param {object} options - Cấu hình
         */
        init(textBlocks, options = {}) {
            _stopAudio();
            _blocks = textBlocks.map(text => ({
                text,
                chunks: _splitChunks(text)
            }));
            _lang         = options.lang         ?? 'vi';
            _rate         = options.rate         ?? 1.0;
            _onBlockStart = options.onBlockStart ?? null;
            _onBlockEnd   = options.onBlockEnd   ?? null;
            _onChapterEnd = options.onChapterEnd ?? null;
            _onStop       = options.onStop       ?? null;
            _onStateChange = options.onStateChange ?? null;
            _blockIdx     = options.startBlock   ?? 0;
            _chunkIdx     = 0;
            _isPlaying    = false;
        },

        /**
         * Bắt đầu phát từ block chỉ định (mặc định từ block hiện tại).
         * @param {number} [blockIndex] - Index đoạn cần phát
         */
        play(blockIndex) {
            if (typeof blockIndex === 'number') {
                _blockIdx = Math.max(0, Math.min(blockIndex, _blocks.length - 1));
                _chunkIdx = 0;
            }
            _isPlaying = true;
            if (_onStateChange) _onStateChange(true);
            _playCurrentChunk();
        },

        /** Tạm dừng phát. */
        pause() {
            _isPlaying = false;
            if (_audio) _audio.pause();
            if (_onStateChange) _onStateChange(false);
        },

        /** Tiếp tục phát sau khi pause. */
        resume() {
            if (!_isPlaying) {
                _isPlaying = true;
                if (_onStateChange) _onStateChange(true);
                if (_audio && _audio.paused && !_audio.ended && _audio.src) {
                    _audio.play().catch(() => {
                        // Audio hết hạn hoặc bị lỗi block, tải lại chunk hiện tại
                        _playCurrentChunk();
                    });
                } else {
                    // Tải lại chunk (audio bị hủy hoặc kết thúc)
                    _playCurrentChunk();
                }
            }
        },

        /** Dừng hẳn và về đầu. */
        stop() {
            _isPlaying = false;
            _stopAudio();
            _blockIdx = 0;
            _chunkIdx = 0;
            if (_onStateChange) _onStateChange(false);
            if (_onStop) _onStop();
        },

        /** Chuyển sang đoạn tiếp theo. */
        next() {
            if (_blockIdx < _blocks.length - 1) this.play(_blockIdx + 1);
        },

        /** Quay lại đoạn trước. */
        prev() {
            if (_blockIdx > 0) this.play(_blockIdx - 1);
        },

        /** Cập nhật ngôn ngữ (áp dụng cho chunk kế tiếp). */
        setLang(lang) { _lang = lang; },

        /** Cập nhật tốc độ đọc (áp dụng ngay lập tức). */
        setRate(rate) {
            _rate = rate;
            if (_audio) _audio.playbackRate = rate;
        },

        // ── Getters ──
        get currentBlock()  { return _blockIdx; },
        get blockCount()    { return _blocks.length; },
        get isPlaying()     { return _isPlaying; },
        get currentLang()   { return _lang; },
        get currentRate()   { return _rate; }
    };
})();
