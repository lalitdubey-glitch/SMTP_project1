$(document).ready(function () {
    $("#save").on("click", function () {
        Save();
    })

    $("#sendOTP").on("click", function () {
        SendOTP();
    })

    $("#vOTP").on("click", function () {
        VerifyOTP();
    })

    $("#cPass").on("click", function () {
        ChangePass();
    })

    $("#Login").on("click", function () {
        Login();
    })

    $("#edit").on("click", function () {
        var id = $("#id").val();
    })


function Save() { 
    var form = new FormData(document.getElementById("SignupForm"))
    $.ajax({
        url: "/home/SignUp",
        type: "post",
        data: form,
        contentType: false,
        processData: false,
        beforeSend: function () {
            $("#loader").show();
        },
        complete: function () {
            $("#loader").hide();
        },
        success: function (res) {
            if (res.success == "update") {
                alert("Data Updated");
                location.href = "/home/Dashboard/"; 
            }
            else if (res.success === true) {
                alert("data saved")
                document.getElementById("SignupForm").reset();
                location.href = "/home/Login/";
                return;
            }
            else {
                alert("Data Not Saved");
                return;
            }
            
        },
        error: function (res) {
            console.log(res);
        }

    })
}
function Login() { 
    var email = $("#email").val()
    var pass = $("#pass").val()
    $.ajax({
        url: "/home/Login",
        type: "post",
        data: { email: email, pass: pass }, 
        beforeSend: function () {
            $("#loader").show();
        },
        complete: function () {
            $("#loader").hide();
        },
        success: function (res) {
            debugger
            if (res.success == "Email Not Exists") {
                alert("You Are Not Registered Please Sign Up First");
                location.href = "/home/SignUp/";
            }
            else if (res.success === true) {
                document.getElementById("LoginForm").reset();
                location.href = "/home/Dashboard/";
            }
            else {
                alert("Email or Password is not mathed");
                return;
            }
        },
        error: function (res) {
            console.log(res);
        }

    })
}
function SendOTP() { 
    var email = $("#email").val()
    debugger
    $.ajax({
        url: "/home/SendOTP",
        type: "post",
        data: { email: email}, 
        beforeSend: function () {
            $("#loader").show();
        },
        complete: function () {
            $("#loader").hide();
        },
        success: function (res) {
            document.getElementById("FgtPass").reset();
            location.href = "/home/VerifyOTP/";
        },
        error: function (res) {
            console.log(res);
        }

    })
}
function VerifyOTP() { 
    var otp = $("#otp").val() 

    $.ajax({
        url: "/home/VerifyOTP",
        type: "post",
        data: { otp: otp}, 
        beforeSend: function () {
            $("#loader").show();
        },
        complete: function () {
            $("#loader").hide();
        },
        success: function (res) {
            document.getElementById("verifyForm").reset();
            location.href = "/home/ChangePass/";
        },
        error: function (res) {
            console.log(res);
        }

    })
}
function ChangePass() { 
    var pass = $("#pass").val() 

    $.ajax({
        url: "/home/ChangePass",
        type: "post",
        data: { pass: pass}, 
        beforeSend: function () {
            $("#loader").show();
        },
        complete: function () {
            $("#loader").hide();
        },
        success: function (res) {
            if (res.success === true) {
                document.getElementById("changePassForm").reset();
                location.href = "/home/Login/";
            }
            else {
                alert("pass not changed")
            }
        },
        error: function (res) {
            console.log(res);
        }

    })
    }

})