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
            + "&query_to_station_name=" + '衡阳'
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

var s = "P4#D704EEA14123DB4A1224F86648F9688672A9C793ADD195B60491791B#rApiyk%2F2cSIccYSuzE3a9bRMbUkO9fhU#1";
var poststr = "passengerTicketStr=3%2C0%2C1%2C%E8%82%96%E6%AF%85%2C1%2C430421198110110033%2C%2CN&oldPassengerStr=%E8%82%96%E6%AF%85%2C1%2C430421198110110033%2C1_&randCode=&purpose_codes=ADULT&key_check_isChange="
    + "D704EEA14123DB4A1224F86648F9688672A9C793ADD195B60491791B" + "&leftTicketStr="
    + "rApiyk%2F2cSIccYSuzE3a9bRMbUkO9fhU" + "&train_location=P4&choose_seats=&seatDetailType=&_json_att=";
$.post("/otn/confirmPassenger/confirmSingleForQueueAsys", poststr,
           function (r) {
               console.log(r);
           });



var secretStr = "0JA66R15Emu1geZj7CdrNtb%2BdsX5mmn1NkDvc6c7srG%2B9vyOL2KjsVSaHDTuV12tvgGGHNHEd8yx%0A6%2Bztgwy6hdyODRUB1om3KkZBYRuWLxXyz%2BpFmY1DWYzLe3%2BqNh%2BvWLqLq%2FTVZ5uCRIwX5YblYnCO%0A2oDgpLM%2BIikZElr7gKxQkbKfC%2ByZpEAO%2Fhpax7AYuyumqmpJGVU50he%2FXwqasmciyaNjeZRTUFdb%0Ak06o5Yq0MYuyf1gaWVkp5PfYV%2BiQ8atFaNAbJ4Jl9UmwvJL3AbBltGR0jcm6ADa37A%3D%3D";
var poststr = "secretStr=" + secretStr + "&train_date=2017-01-02&tour_flag=dc&purpose_codes=ADULT&query_from_station_name=北京&query_to_station_name=桂林&&cancel_flag=2&bed_level_order_num=000000000000000000000000000000&passengerTicketStr=3,0,1,肖毅,1,430421198110110033,,N&oldPassengerStr=肖毅,1,430421198110110033,1_";
$.post("/otn/confirmPassenger/autoSubmitOrderRequest", poststr,
           function (r) {
               console.log(r);
           });