(function() {
    let pickerEnabled = false;
    let currentHover = null;
    let overlay = null;

    // Lắng nghe lệnh từ Parent window (Cửa sổ mẹ)
    window.addEventListener('message', function(e) {
        if (e.data && e.data.action === 'enablePicker') {
            pickerEnabled = true;
            createOverlay();
            // Đảm bảo body có position relative
            if(window.getComputedStyle(document.body).position === 'static') {
                document.body.style.position = 'relative';
            }
        }
    });

    function createOverlay() {
        if(!overlay) {
            overlay = document.createElement('div');
            overlay.style.position = 'absolute';
            overlay.style.backgroundColor = 'rgba(59, 130, 246, 0.3)'; // blue tint
            overlay.style.border = '2px solid #3b82f6';
            overlay.style.pointerEvents = 'none';
            overlay.style.zIndex = '999999';
            overlay.style.display = 'none';
            overlay.style.transition = 'all 0.1s';
            document.body.appendChild(overlay);
        }
    }

    function updateOverlay(rect) {
        if(!overlay) return;
        let scrollX = window.scrollX || document.documentElement.scrollLeft;
        let scrollY = window.scrollY || document.documentElement.scrollTop;
        
        overlay.style.left = (rect.left + scrollX) + 'px';
        overlay.style.top = (rect.top + scrollY) + 'px';
        overlay.style.width = rect.width + 'px';
        overlay.style.height = rect.height + 'px';
        overlay.style.display = 'block';
    }

    function getUniqueSelector(el) {
        if (!el || el.nodeType !== 1) return '';
        
        // Nếu có ID, dùng ID là ngon nhất
        if (el.id) {
            return '#' + el.id;
        }

        // Tạo selector dựa trên tag và class
        let selector = el.tagName.toLowerCase();
        
        if (el.className && typeof el.className === 'string') {
            let classes = el.className.trim().split(/\s+/).filter(c => c);
            if (classes.length > 0) {
                // Chọn class đầu tiên hoặc class dài nhất làm mốc để hạn chế trùng lặp
                // Tránh các class chung chung như "active", "btn"
                let validClasses = classes.filter(c => c.length > 2 && !['active','btn','container','row','col','p-1','m-1'].includes(c));
                if(validClasses.length > 0) {
                    selector += '.' + validClasses.join('.');
                }
            }
        }

        // Lấy nth-child nếu bị trùng (Kiểm tra xem selector này có duy nhất không)
        try {
            if (document.querySelectorAll(selector).length === 1) {
                return selector;
            }
        } catch(e) {}

        // Nếu không duy nhất, tìm cha của nó
        if (el.parentElement && el.parentElement !== document.body && el.parentElement !== document.documentElement) {
            let parentSelector = getUniqueSelector(el.parentElement);
            if(parentSelector) {
                selector = parentSelector + ' > ' + selector;
            }
        }
        
        return selector;
    }

    // Sự kiện hover
    document.addEventListener('mouseover', function(e) {
        if (!pickerEnabled) return;
        
        // Bỏ qua chính cái overlay
        if (e.target === overlay) return;

        currentHover = e.target;
        let rect = currentHover.getBoundingClientRect();
        updateOverlay(rect);
        
        e.stopPropagation();
    }, true);

    document.addEventListener('mouseout', function(e) {
        if (!pickerEnabled) return;
        if(overlay) overlay.style.display = 'none';
    }, true);

    // Sự kiện click
    document.addEventListener('click', function(e) {
        if (!pickerEnabled) return;
        
        e.preventDefault();
        e.stopPropagation();

        let el = e.target;
        let selector = getUniqueSelector(el);

        // Gửi kết quả về cho Parent
        window.parent.postMessage({
            type: 'selectorPicked',
            selector: selector
        }, '*');

        pickerEnabled = false;
        if(overlay) overlay.style.display = 'none';

    }, true);
})();
