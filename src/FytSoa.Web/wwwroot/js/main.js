// Main site interactions for 利鑫康养官网（jQuery 版本）
$(function () {
    // 缓存常用节点
    var $body = $('body');
    var $window = $(window);
    var $mobileToggle = $('#mobile-menu-toggle');
    var $navLinks = $('#nav-links');
    var $heroSlides = $('.hero-carousel-slide');
    var $heroBadge = $('#hero-badge');
    var $heroTitle = $('#hero-title');
    var $heroDesc = $('#hero-desc');
    var $heroIndicatorsContainer = $('.hero-carousel-indicators');
    var $productMainImage = $('#product-main-image');
    var $productThumbs = $('.product-gallery-thumb');
    var $productZoomOverlay = $('#product-image-zoom');
    var $productZoomImage = $productZoomOverlay.find('img');
    var $productZoomClose = $productZoomOverlay.find('[data-zoom-close]');
    var $productZoomBtn = $('.product-gallery-zoom-btn');

    function setBodyScrollLock(locked) {
        $body.css('overflow', locked ? 'hidden' : '');
    }

    function isDesktop() {
        return window.innerWidth >= 1024;
    }

    function syncNavToViewport() {
        if (!$navLinks.length) return;
        if (isDesktop()) {
            $navLinks.removeClass('hidden');
            setBodyScrollLock(false);
        } else {
            $navLinks.addClass('hidden');
        }
    }

    // 顶部导航：移动端菜单开关
    if ($mobileToggle.length && $navLinks.length) {
        syncNavToViewport();

        $mobileToggle.on('click', function () {
            var willOpen = $navLinks.hasClass('hidden');
            $navLinks.toggleClass('hidden');
            if (!isDesktop()) {
                setBodyScrollLock(willOpen);
            }
        });

        // 点击链接自动收起移动端菜单
        $navLinks.on('click', 'a', function () {
            if (isDesktop()) return;
            $navLinks.addClass('hidden');
            setBodyScrollLock(false);
        });

        $window.on('resize', syncNavToViewport);
    }

    // 产品详情：图片画廊（大图 + 缩略图）
    if ($productMainImage.length && $productThumbs.length) {
        $productThumbs.on('click', function () {
            var $thumb = $(this);
            var largeSrc = $thumb.data('large');
            if (!largeSrc) return;

            $productMainImage.attr('src', largeSrc);
            $productThumbs.removeClass('is-active');
            $thumb.addClass('is-active');
        });
    }

    // 产品详情：图片放大预览
    function openProductZoom() {
        if (!$productZoomOverlay.length || !$productZoomImage.length || !$productMainImage.length) return;
        $productZoomImage.attr('src', $productMainImage.attr('src'));
        $productZoomOverlay.removeClass('hidden');
        setBodyScrollLock(true);
    }

    function closeProductZoom() {
        if (!$productZoomOverlay.length) return;
        $productZoomOverlay.addClass('hidden');
        setBodyScrollLock(false);
    }

    if ($productZoomBtn.length && $productZoomOverlay.length) {
        $productZoomBtn.on('click', function (e) {
            e.stopPropagation();
            openProductZoom();
        });
    }

    if ($productMainImage.length && $productZoomOverlay.length) {
        $productMainImage.on('click', openProductZoom);
    }

    if ($productZoomClose.length && $productZoomOverlay.length) {
        $productZoomClose.on('click', function (e) {
            e.preventDefault();
            closeProductZoom();
        });
    }

    if ($productZoomOverlay.length) {
        $productZoomOverlay.on('click', function (e) {
            if (e.target === this) {
                closeProductZoom();
            }
        });
    }

    // Hero 轮播图（首屏）
    if ($heroSlides.length > 1) {
        var currentHeroIndex = 0;
        var HERO_INTERVAL = 6000;
        var heroTimer = null;
        var heroIndicators = [];

        function updateHeroContent(index) {
            if (!$heroBadge.length || !$heroTitle.length || !$heroDesc.length) return;
            var $slide = $heroSlides.eq(index);
            if (!$slide.length) return;

            // 从当前轮播图节点读取文案（避免文案写死在 JS 中）
            var badge = $slide.data('badge');
            var title = $slide.data('title');
            var desc = $slide.data('desc');

            if (badge) {
                $heroBadge.text(badge);
            }
            if (title) {
                $heroTitle.html(title);
            }
            if (desc) {
                $heroDesc.text(desc);
            }
        }

        function goToHeroSlide(nextIndex) {
            if (nextIndex === currentHeroIndex) return;
            $heroSlides.eq(currentHeroIndex).removeClass('is-active');
            if (heroIndicators[currentHeroIndex]) {
                heroIndicators[currentHeroIndex].removeClass('is-active');
            }
            currentHeroIndex = nextIndex;
            $heroSlides.eq(currentHeroIndex).addClass('is-active');
            if (heroIndicators[currentHeroIndex]) {
                heroIndicators[currentHeroIndex].addClass('is-active');
            }
            updateHeroContent(currentHeroIndex);
        }

        function startHeroTimer() {
            if (heroTimer) clearInterval(heroTimer);
            heroTimer = setInterval(function () {
                var nextIndex = (currentHeroIndex + 1) % $heroSlides.length;
                goToHeroSlide(nextIndex);
            }, HERO_INTERVAL);
        }

        if ($heroIndicatorsContainer.length) {
            $heroSlides.each(function (index) {
                var $dot = $('<button/>', {
                    type: 'button',
                    'class': 'hero-carousel-indicator' + (index === 0 ? ' is-active' : ''),
                    'aria-label': '切换到第 ' + (index + 1) + ' 张轮播图'
                });
                $dot.on('click', function () {
                    goToHeroSlide(index);
                    startHeroTimer();
                });
                $heroIndicatorsContainer.append($dot);
                heroIndicators.push($dot);
            });
        }

        startHeroTimer();

        // 初始化首屏文案（防止内容与第一张轮播错位）
        updateHeroContent(0);
    }

    // 返回顶部按钮
    var $scrollTopBtn = $('#scroll-top-btn');
    if ($scrollTopBtn.length) {
        $scrollTopBtn.on('click', function () {
            $('html, body').animate({ scrollTop: 0 }, 400);
        });
    }

    // 导出当前页面 HTML（保留外链）
    var $exportBtn = $('#export-html-btn');
    if ($exportBtn.length) {
        $exportBtn.on('click', function () {
            try {
                var htmlContent = '<!DOCTYPE html>\n' + document.documentElement.outerHTML;
                var blob = new Blob([htmlContent], { type: 'text/html' });
                var url = URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;
                a.download = 'LixinHealth_Standalone.html';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            } catch (e) {
                console.error('导出 HTML 失败:', e);
                alert('导出 HTML 失败，请尝试使用浏览器的“另存为”功能。');
            }
        });
    }
});
