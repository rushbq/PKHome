//選單固定置頂
$('.ui.sticky').sticky({
    container: $('#myStickyBody'),
    offset: 51
});


/* 當網址有#錨點時,觸發以下動作 */
if (window.location.hash) {
    var
      hash = window.location.hash,
      $element = $(window.location.hash),
      position = $element.offset().top - 25
    ;

    //巡覽fastjump menu, 若有相同的hash,則加入active
    $("#fastjump a").each(function () {
        var menuhash = $(this).attr("href");
        if (hash == menuhash) {
            $(this).addClass('active')
        }
    });
    

    $('html, body')
      .stop()
      .animate({
          scrollTop: position
      }, 400)
    ;
}

/* 快速選單按下後,該項目active */
const menuNav = $('#fastjump .item');
menuNav.on('click', function (item) {
    menuNav.removeClass('active');
    $(this).addClass('active');

    var
     id = $(this).attr('href').replace('#', ''),
     $element = $('#' + id),
     position = $element.offset().top - 25
    ;
    
    $('html, body')
      .stop()
      .animate({
          scrollTop: position
      }, 400)
    ;
});