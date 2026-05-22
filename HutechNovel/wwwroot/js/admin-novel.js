$(document).ready(function () {

    // =============================================
    // TAG SYSTEM (Custom, không dùng Select2)
    // =============================================
   var selectedTags = {}; // { id: text } — id có thể là số (DB) hoặc string (tag mới)

    // Khởi tạo từ các chip đã được render sẵn bởi Razor (dùng cho trang Sửa)
    $('#selectedTagsContainer .tag-chip-item').each(function() {
        // DÙNG .attr() THAY VÌ .data() ĐỂ CHỐNG LỖI CRASH JS
        var id = $(this).attr('data-id');
        var txt = $(this).attr('data-text');
        
        if (id && txt) {
            selectedTags[id] = txt;
        }
    });
    function renderChips() {
        $('#selectedTagsContainer').empty();
        $.each(selectedTags, function (id, text) {
            $('#selectedTagsContainer').append(
                `<span class="tag-chip tag-chip-item me-2 mb-2" data-id="${id}" data-text="${text}" style="cursor:default;">
                    ${text}
                    <button type="button" class="btn-remove-tag ms-1"
                            style="background:none;border:none;color:inherit;cursor:pointer;font-weight:bold;padding:0 2px;"
                            data-id="${id}">×</button>
                    <input type="hidden" name="selectedTags" value="${id}" />
                </span>`
            );
        });
    }

    // Render lần đầu (cho trang Sửa có dữ liệu sẵn)
    renderChips();

    // Xóa tag khi bấm ×
    $(document).on('click', '.btn-remove-tag', function () {
        var id = $(this).data('id').toString();
        delete selectedTags[id];
        renderChips();
    });

    // Tìm kiếm tag AJAX
    let tagTimeout;
    $('#inputTimThe').on('input', function () {
        clearTimeout(tagTimeout);
        var keyword  = $(this).val().trim();
        var dropdown = $('#dropdownThe');

        if (keyword.length < 1) {
            dropdown.hide();
            return;
        }

        tagTimeout = setTimeout(function () {
            $.ajax({
                url: '/Admin/QuanLyTruyen/SearchThe',
                data: { keyword: keyword },
                success: function (data) {
                    dropdown.empty();

                    data.forEach(function (item) {
                        var alreadySelected = selectedTags.hasOwnProperty(item.id.toString());
                        if (!alreadySelected) {
                            dropdown.append(
                                `<li data-id="${item.id}" data-text="${item.text}">${item.text}</li>`
                            );
                        }
                    });

                    // Nếu không match chính xác → cho phép tạo mới
                    var exactMatch = data.some(function (d) {
                        return d.text.toLowerCase() === keyword.toLowerCase();
                    });
                    if (!exactMatch) {
                        dropdown.append(
                            `<li data-id="new_${keyword}" data-text="${keyword}" style="font-style:italic;">
                                Thêm mới: "${keyword}"
                            </li>`
                        );
                    }

                    dropdown.children().length > 0 ? dropdown.show() : dropdown.hide();
                }
            });
        }, 300);
    });

    // Chọn tag từ dropdown
    $(document).on('click', '#dropdownThe li[data-id]', function () {
        var id   = $(this).data('id').toString();
        var text = $(this).data('text');

        selectedTags[id] = text;
        renderChips();

        $('#inputTimThe').val('');
        $('#dropdownThe').hide();
    });

    // Bấm Enter để thêm tag đang gõ
    $('#inputTimThe').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            var keyword = $(this).val().trim();
            if (!keyword) return;

            var id = 'new_' + keyword;
            if (!selectedTags.hasOwnProperty(id)) {
                selectedTags[id] = keyword;
                renderChips();
            }
            $(this).val('');
            $('#dropdownThe').hide();
        }
    });

    // Bấm ra ngoài thì ẩn dropdown tag
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#inputTimThe, #dropdownThe').length) {
            $('#dropdownThe').hide();
        }
    });

    // =============================================
    // TÁC GIẢ AUTOCOMPLETE (giữ nguyên)
    // =============================================
    let timeoutId;
    $('#inputTenTacGia').on('input', function () {
        clearTimeout(timeoutId);
        var keyword  = $(this).val();
        var dropdown = $('#dropdownTacGia');
        $('#hiddenMaTacGia').val('0');

        if (keyword.length < 1) { dropdown.hide(); return; }

        timeoutId = setTimeout(function () {
            $.ajax({
                url: '/Admin/QuanLyTruyen/SearchTacGia',
                data: { keyword: keyword },
                success: function (data) {
                    dropdown.empty();
                    if (data.length > 0) {
                        data.forEach(function (item) {
                            dropdown.append(`<li data-id="${item.id}">${item.text}</li>`);
                        });
                        dropdown.show();
                    } else {
                        dropdown.append(`<li class="text-muted" style="cursor:default;">Không tìm thấy, sẽ tạo mới</li>`);
                        dropdown.show();
                    }
                }
            });
        }, 300);
    });

    $(document).on('click', '#dropdownTacGia li[data-id]', function () {
        var id = $(this).data('id');
        if (id) {
            $('#inputTenTacGia').val($(this).text());
            $('#hiddenMaTacGia').val(id);
            $('#dropdownTacGia').hide();
        }
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#inputTenTacGia, #dropdownTacGia').length) {
            $('#dropdownTacGia').hide();
        }
    });

    // =============================================
    // ẢNH BÌA — vô hiệu hóa chéo
    // =============================================
    $('#fileAnhBia').on('change', function () {
        if ($(this).val()) { $('#linkAnhBia').val(''); }
    });
    $('#linkAnhBia').on('input', function () {
        if ($(this).val()) { $('#fileAnhBia').val(''); }
    });
});