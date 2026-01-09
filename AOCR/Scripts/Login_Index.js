// Login_Index.js

$(document).ready(function () {
    console.log("Login_Index.js cargado correctamente.");

    // Mostrar u ocultar la contraseña
    $('#toggleIcon').on('click', function () {
        const input = $('#clave');
        const icon = $(this);

        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // Enviar solicitud de recuperación de contraseña
    $('#formRecuperarClave').on('submit', function (e) {
        e.preventDefault();
        const email = $('#correoRecuperacion').val();

        if (email === '') {
            alert("Debe ingresar un correo electrónico.");
            return;
        }

        // Aquí deberías hacer una llamada AJAX real al servidor
        alert("Se ha enviado un correo a: " + email);
        $('#forgotPasswordModal').modal('hide');
    });
});
