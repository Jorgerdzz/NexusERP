window.SweetAlertService = {

    success: async function (title, text, redirectUrl = null) {

        await Swal.fire({
            icon: "success",
            title: title,
            text: text,
            confirmButtonText: "Aceptar",
            confirmButtonColor: "#3085d6"
        });

        if (redirectUrl) {
            window.location.href = redirectUrl;
        }
    },

    error: function (title, text) {

        Swal.fire({
            icon: "error",
            title: title,
            text: text,
            confirmButtonColor: "#d33"
        });
    },

    warning: function (title, text) {

        Swal.fire({
            icon: "warning",
            title: title,
            text: text
        });
    },

    confirm: async function (title, text) {

        const result = await Swal.fire({
            icon: "question",
            title: title,
            text: text,
            showCancelButton: true,
            confirmButtonText: "Confirmar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#3085d6"
        });

        return result.isConfirmed;
    },

    loading: function (text = "Procesando...") {

        Swal.fire({
            title: text,
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

    }

};