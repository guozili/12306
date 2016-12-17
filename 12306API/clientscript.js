//TODO setting.txt里内容通过ajax传过去
//TODO 更进一步植入到页面里去，提供设置框的界面

$.ajax({
    url: "https://127.0.0.1/",
    dataType: "jsonp",
    jsonp: "callback",
    success: function (data) {
        var poststr =
            "secretStr=" + data
            + "&train_date=" + '2017-01-05'
            + "&back_train_date=" + '2016-12-07'
            + "&tour_flag=" + 'dc'
            + "&purpose_codes=" + 'ADULT'
            + "&query_from_station_name=" + '北京西'
            + "&query_to_station_name=" + '桂林'
        $.post("/otn/leftTicket/submitOrderRequest", poststr,
            function (r) {
                console.log(r);
                window.location.href = "/otn/confirmPassenger/initDc";
            });
    },
    error: function () {
        console.log("fail");
    }
});

//发现上面的几个参数不用传，简化：
$.ajax({
    url: "https://127.0.0.1/",
    data: { "ReturnSecretStr": "true" },
    dataType: "jsonp",
    jsonp: "callback",
    success: function (data) {
        var poststr =
            "secretStr=" + data
            + "&tour_flag=" + 'dc'
            + "&purpose_codes=" + 'ADULT'
        $.post("/otn/leftTicket/submitOrderRequest", poststr,
            function (r) {
                console.log(r);
                window.location.href = "/otn/confirmPassenger/initDc";
            });
    },
    error: function () {
        console.log("fail");
    }
});


var prepared = false;
var names = ["王倩", "刘美颖"];
$("#seatType_1").val(3);
setInterval(function () {
    if (!prepared) {
        var passengers = $("label[for^=normalPassenger]");
        if (passengers.length > 1) {
            passengers.each(function () {
                if (names.indexOf($(this).text()) != -1) {
                    this.click();
                }
            });

            $("#submitOrder_id")[0].click();
            prepared = true;
        }
    }
}, 300);



var postStr = "";
postStr += $("#fromStationText").val() + "\r\n"
postStr += $("#toStationText").val() + "\r\n"
$("#prior_train .sel-box").each(function () { postStr += $(this).text() + "," });
postStr = postStr.substring(0, postStr.length - 1) + "\r\n"
postStr += $("#train_date").val() + "\r\n"
$("#setion_postion .sel-box").each(function () { postStr += $(this).text().split("(")[0] + "," });
postStr = postStr.substring(0, postStr.length - 1) + "\r\n"
$("#prior_seat .sel-box").each(function () { postStr += $(this).text() + "," });
postStr = postStr.substring(0, postStr.length - 1)
console.log(postStr);
$.ajax({
    url: "https://127.0.0.1/",
    data: { "ReturnResult": "true", "postStr": postStr },
    dataType: "jsonp",
    jsonp: "callback",
    success: function (data) {
        window.piaoresult = data;
        $("#query_ticket")[0].click();
    },
    error: function () {
        console.log("fail");
    }
});
$.ajaxSetup({
    dataFilter: function (data, type) {
        if (window.piaoresult && window.piaoresult != "" && data.indexOf("queryLeftNewDTO") > 0) {
            
            data = window.piaoresult;
            window.piaoresult = "";
        }

        return data;
    }
});

