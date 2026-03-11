document.addEventListener("DOMContentLoaded", function () {

    const form = document.querySelector("#modal-add-department form");

    if (!form) return;

    form.addEventListener("submit", async function (e) {

        e.preventDefault();

        const btn = document.getElementById("btnCrearDepartamento");

        const urlPost = btn.dataset.urlPost;
        const urlRedirect = btn.dataset.urlRedirect;

        const formData = new FormData(form);

        const nombre = formData.get("Nombre");
        const presupuesto = formData.get("PresupuestoAnual");

        if (!nombre || !presupuesto) {

            SweetAlertService.warning(
                "Campos obligatorios",
                "Debes rellenar todos los campos."
            );

            return;
        }

        const confirmado = await SweetAlertService.confirm(
            "Crear departamento",
            "¿Deseas crear este departamento?"
        );

        if (!confirmado) return;

        SweetAlertService.loading("Creando departamento...");

        try {

            const response = await fetch(urlPost, {
                method: "POST",
                body: formData
            });

            if (response.ok) {

                SweetAlertService.success(
                    "Departamento creado",
                    "El departamento se ha creado correctamente.",
                    urlRedirect
                );

            } else {

                SweetAlertService.error(
                    "Error",
                    "No se pudo crear el departamento."
                );

            }

        } catch {

            SweetAlertService.error(
                "Error inesperado",
                "Ha ocurrido un error en la petición."
            );

        }

    });

});