// crearUsuario.js

$(document).ready(function () {
    console.log("crearUsuario.js cargado correctamente.");

    // Ejemplo de envío de formulario de registro
    $('#formCrearUsuario').on('submit', function (e) {
        e.preventDefault();

        const correo = $('#correoElectronico').val();
        const nombres = $('#nombres').val();
        const apellidos = $('#apellidos').val();

        if (!correo || !nombres || !apellidos) {
            alert("Debe llenar todos los campos obligatorios.");
            return;
        }

        // Aquí también debería ir una llamada AJAX al servidor para guardar el usuario
        alert("Usuario registrado con éxito: " + nombres + " " + apellidos);

        $('#modalCrearUsuario').modal('hide');
    });
});
