/**
 * HutechNovel TTS UI v2
 * Quản lý: Mini Player (sticky bottom), Sleep Timer, Voice Control (SpeechRecognition), Smart Resume
 * Phụ thuộc: tts-engine.js (TTSEngine phải được load trước)
 */

const TTSMiniPlayer = (() => {
    // ── Constants ─────────────────────────────────────────────────────────────
    const RESUME_KEY = 'hutechnovel-tts-resume';

    // ── Sleep Timer Options (giây) ─────────────────────────────────────────────
    const SLEEP_OPTIONS = [
        { label: 'Không', seconds: 0 },
        { label: '5 phút', seconds: 300 },
        { label: '10 phút', seconds: 600 },
        { label: '15 phút', seconds: 900 },
        { label: 'Hết chương', seconds: -1 },
    ];

    // ── State ─────────────────────────────────────────────────────────────────
    let _engine        = null;
    let _chapterId     = null;
    let _totalBlocks   = 0;
    let _storyTitle    = '';
    let _chapterTitle  = '';

    // Sleep timer
    let _sleepRemaining = 0; // giây còn lại (0 = tắt)
    let _sleepMode      = 0; // 0=off, >0=giây, -1=hết chương
    let _sleepInterval  = null;
    let _sleepOptionIdx = 0;

    // Voice control
    let _recognition    = null;
    let _voiceActive    = false;

    // DOM refs (inject vào body)
    let _player = null;

    // ── DOM Injection ─────────────────────────────────────────────────────────
    function _inject() {
        _player = document.createElement('div');
        _player.id = 'ttsMiniplayer';
        _player.setAttribute('role', 'region');
        _player.setAttribute('aria-label', 'Trình phát Audiobook');
        _player.innerHTML = `
            <div class="tts-mp-info">
                <span class="tts-mp-title" id="mpTitle"></span>
                <span class="tts-mp-chapter" id="mpChapter"></span>
            </div>
            <div class="tts-mp-controls">
                <button id="mpPrev"      title="Đoạn trước"><i class="fas fa-backward-step"></i></button>
                <button id="mpPlayPause" title="Phát / Tạm dừng"><i class="fas fa-play"></i></button>
                <button id="mpNext"      title="Đoạn sau"><i class="fas fa-forward-step"></i></button>
                <button id="mpStop"      title="Dừng hẳn"><i class="fas fa-square"></i></button>
            </div>
            <div class="tts-mp-extras">
                <button id="mpSleepTimer" title="Hẹn giờ tắt">
                    <i class="fas fa-moon"></i>
                    <span id="mpTimerLabel"></span>
                </button>
                <button id="mpVoiceCtrl" title="Điều khiển bằng giọng nói (Chrome/Edge)">
                    <i class="fas fa-microphone-slash"></i>
                </button>
                <button id="mpClose" title="Đóng mini player">
                    <i class="fas fa-chevron-down"></i>
                </button>
            </div>
            <div class="tts-mp-progressbar" id="mpProgressBar"></div>
        `;
        document.body.appendChild(_player);
        _bindPlayerEvents();
    }

    // ── Player Event Bindings ─────────────────────────────────────────────────
    function _bindPlayerEvents() {
        document.getElementById('mpPrev').addEventListener('click', () => {
            _engine.prev();
        });

        document.getElementById('mpPlayPause').addEventListener('click', () => {
            if (_engine.isPlaying) {
                _engine.pause();
            } else {
                _engine.resume();
            }
        });

        document.getElementById('mpNext').addEventListener('click', () => {
            _engine.next();
        });

        document.getElementById('mpStop').addEventListener('click', () => {
            _engine.stop();
            hide();
        });

        // Cycle qua các sleep options
        document.getElementById('mpSleepTimer').addEventListener('click', () => {
            _sleepOptionIdx = (_sleepOptionIdx + 1) % SLEEP_OPTIONS.length;
            const opt = SLEEP_OPTIONS[_sleepOptionIdx];
            _setSleepTimer(opt.seconds, opt.label);
        });

        document.getElementById('mpVoiceCtrl').addEventListener('click', () => {
            _toggleVoiceControl();
        });

        document.getElementById('mpClose').addEventListener('click', () => {
            hide();
        });
    }

    // ── Player UI Updates ─────────────────────────────────────────────────────
    function _updatePlayBtn(isPlaying) {
        const btn = document.getElementById('mpPlayPause');
        if (!btn) return;
        btn.innerHTML = isPlaying
            ? '<i class="fas fa-pause"></i>'
            : '<i class="fas fa-play"></i>';
        btn.classList.toggle('is-active', isPlaying);
    }

    function _updateProgress(blockIdx) {
        const total = _totalBlocks;
        const chapterEl = document.getElementById('mpChapter');
        const bar = document.getElementById('mpProgressBar');

        if (chapterEl) chapterEl.textContent = `${_chapterTitle} · Đoạn ${blockIdx + 1}/${total}`;
        if (bar) bar.style.width = total > 0 ? `${((blockIdx + 1) / total) * 100}%` : '0%';
    }

    // ── Sleep Timer ───────────────────────────────────────────────────────────
    function _setSleepTimer(seconds, label) {
        // Xóa timer cũ
        if (_sleepInterval) clearInterval(_sleepInterval);
        _sleepRemaining = seconds;
        _sleepMode = seconds;

        const timerLabel = document.getElementById('mpTimerLabel');
        const btn = document.getElementById('mpSleepTimer');
        if (!btn) return;

        if (seconds === 0) {
            if (timerLabel) timerLabel.textContent = '';
            btn.classList.remove('is-active');
            return;
        }

        btn.classList.add('is-active');

        if (seconds === -1) {
            // Chế độ "Hết chương" - xử lý trong onChapterEnd callback
            if (timerLabel) timerLabel.textContent = 'Hết chương';
            return;
        }

        // Đếm ngược theo giây
        if (timerLabel) timerLabel.textContent = _formatTime(seconds);

        _sleepInterval = setInterval(() => {
            _sleepRemaining--;
            if (timerLabel) timerLabel.textContent = _formatTime(_sleepRemaining);

            if (_sleepRemaining <= 0) {
                clearInterval(_sleepInterval);
                _sleepInterval = null;
                _sleepOptionIdx = 0;
                if (timerLabel) timerLabel.textContent = '';
                btn.classList.remove('is-active');
                _engine.pause(); // Tắt khi hết giờ
            }
        }, 1000);
    }

    function _formatTime(seconds) {
        const m = Math.floor(seconds / 60);
        const s = seconds % 60;
        return `${m}:${s.toString().padStart(2, '0')}`;
    }

    // ── Smart Resume ──────────────────────────────────────────────────────────
    function saveResumePosition(blockIndex) {
        if (!_chapterId) return;
        localStorage.setItem(RESUME_KEY, JSON.stringify({ chapterId: _chapterId, blockIndex }));
    }

    function getResumePosition() {
        try {
            const saved = JSON.parse(localStorage.getItem(RESUME_KEY) || '{}');
            if (saved.chapterId === _chapterId && saved.blockIndex > 0) {
                return saved.blockIndex;
            }
        } catch {}
        return null;
    }

    function clearResumePosition() {
        localStorage.removeItem(RESUME_KEY);
    }

    // ── Voice Control ─────────────────────────────────────────────────────────
    function _toggleVoiceControl() {
        if (!_voiceActive) {
            _startVoiceControl();
        } else {
            _stopVoiceControl();
        }
    }

    function _startVoiceControl() {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            alert('Trình duyệt này không hỗ trợ điều khiển giọng nói. Vui lòng dùng Chrome hoặc Edge.');
            return;
        }

        _recognition = new SpeechRecognition();
        _recognition.lang = 'vi-VN';
        _recognition.continuous = true;
        _recognition.interimResults = false;

        _recognition.onresult = (event) => {
            const cmd = event.results[event.results.length - 1][0].transcript.toLowerCase().trim();
            console.log('[VoiceCtrl] Lệnh:', cmd);

            if (cmd.includes('tạm dừng') || cmd.includes('dừng lại')) {
                _engine.pause();
            } else if (cmd.includes('đọc tiếp') || cmd.includes('tiếp tục') || cmd.includes('phát')) {
                _engine.resume();
            } else if (cmd.includes('chương sau')) {
                document.getElementById('btnNextChapterBottom')?.click();
            } else if (cmd.includes('dừng hẳn') || cmd.includes('tắt âm')) {
                _engine.stop();
                hide();
            } else if (cmd.includes('đoạn sau') || cmd.includes('tiếp theo')) {
                _engine.next();
            } else if (cmd.includes('đoạn trước') || cmd.includes('quay lại')) {
                _engine.prev();
            }
        };

        _recognition.onerror = (e) => {
            console.warn('[VoiceCtrl] Lỗi:', e.error);
            if (e.error !== 'no-speech') _stopVoiceControl();
        };

        _recognition.onend = () => {
            // Tự khởi động lại nếu vẫn đang bật
            if (_voiceActive) {
                try { _recognition.start(); } catch {}
            }
        };

        _recognition.start();
        _voiceActive = true;
        _updateVoiceBtn();
    }

    function _stopVoiceControl() {
        if (_recognition) {
            try { _recognition.stop(); } catch {}
            _recognition = null;
        }
        _voiceActive = false;
        _updateVoiceBtn();
    }

    function _updateVoiceBtn() {
        const btn = document.getElementById('mpVoiceCtrl');
        if (!btn) return;
        btn.innerHTML = _voiceActive
            ? '<i class="fas fa-microphone" style="color:var(--public-green)"></i>'
            : '<i class="fas fa-microphone-slash"></i>';
        btn.classList.toggle('is-active', _voiceActive);
        btn.title = _voiceActive ? 'Đang nghe lệnh (bấm để tắt)' : 'Điều khiển bằng giọng nói (Chrome/Edge)';
    }

    // ── Show / Hide ───────────────────────────────────────────────────────────
    function show() {
        if (_player) _player.classList.add('is-visible');
    }

    function hide() {
        if (_player) _player.classList.remove('is-visible');
        _stopVoiceControl();
        // Giữ timer chạy nếu user chỉ đóng mini player
    }

    // ── Public API ─────────────────────────────────────────────────────────────
    return {
        /**
         * Khởi tạo mini player và kết nối với TTSEngine.
         * @param {object} engine - Instance của TTSEngine
         * @param {object} options
         * @param {number} options.chapterId
         * @param {number} options.totalBlocks
         * @param {string} options.storyTitle
         * @param {string} options.chapterTitle
         * @param {function} options.onBlockStart - Callback từ Index.cshtml để sync UI panel
         */
        init(engine, options = {}) {
            _engine       = engine;
            _chapterId    = options.chapterId    ?? null;
            _totalBlocks  = options.totalBlocks  ?? 0;
            _storyTitle   = options.storyTitle   ?? '';
            _chapterTitle = options.chapterTitle ?? '';

            _inject();

            // Set title
            const titleEl = document.getElementById('mpTitle');
            if (titleEl) titleEl.textContent = _storyTitle;

            _updateProgress(0);
        },

        /** Gọi khi engine bắt đầu đọc 1 đoạn mới. */
        onBlockStart(blockIdx) {
            show();
            _updateProgress(blockIdx);
            _updatePlayBtn(true);
            saveResumePosition(blockIdx);
        },

        /** Gọi khi engine thay đổi trạng thái play/pause. */
        onStateChange(isPlaying) {
            _updatePlayBtn(isPlaying);
            if (!isPlaying && _voiceActive) {
                // Không dừng voice control khi pause
            }
        },

        /** Gọi khi engine kết thúc chương. */
        onChapterEnd() {
            _updatePlayBtn(false);
            clearResumePosition();
            // Nếu đang ở chế độ "Hết chương" sleep
            if (_sleepMode === -1) {
                if (_sleepInterval) clearInterval(_sleepInterval);
                _sleepOptionIdx = 0;
                const timerLabel = document.getElementById('mpTimerLabel');
                if (timerLabel) timerLabel.textContent = '';
                document.getElementById('mpSleepTimer')?.classList.remove('is-active');
            }
        },

        /** Gọi khi engine stop hẳn. */
        onStop() {
            _updatePlayBtn(false);
            clearResumePosition();
        },

        /** Kiểm tra xem có vị trí resume không. */
        checkResume: getResumePosition,

        /** Xóa vị trí resume đã lưu. */
        clearResume: clearResumePosition,

        show,
        hide
    };
})();
