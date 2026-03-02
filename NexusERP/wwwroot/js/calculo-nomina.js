/* --- wwwroot/js/calculo-nomina.js --- */

document.addEventListener('DOMContentLoaded', function () {

    const inputsDevengos = document.querySelectorAll('.input-devengo');
    const inputSalarioBase = document.querySelector('input[name="SalarioBase"]');
    const porcentajeIrpf = parseFloat(document.getElementById('porcentajeIrpfAplicado').value) || 0;
    const salarioSugeridoText = document.getElementById('salarioSugerido');

    // Auto-rellenar salario base si está vacío
    if (inputSalarioBase && salarioSugeridoText) {
        let sugeridoStr = salarioSugeridoText.innerText.replace(',', '.').replace(/\s/g, '');
        let sugeridoVal = parseFloat(sugeridoStr) || 0;
        if (inputSalarioBase.value === "" || inputSalarioBase.value === "0") {
            inputSalarioBase.value = sugeridoVal.toFixed(2);
        }
    }

    // Escuchar tecleos
    inputsDevengos.forEach(input => {
        input.addEventListener('input', calcularNomina);
    });

    // Calcular al entrar
    calcularNomina();

    function formatMoneda(val) {
        return val.toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function calcularNomina() {
        let totalDevengado = 0;
        let baseIRPF = 0;
        let baseCC = 0;

        // 1. Sumar devengos
        inputsDevengos.forEach(input => {
            const importe = parseFloat(input.value) || 0;
            totalDevengado += importe;

            if (input.dataset.tributaIrpf === "true") {
                baseIRPF += importe;
                baseCC += importe;
            }
        });

        // 2. Prorrateo 2 pagas extra
        const valBase = inputSalarioBase ? (parseFloat(inputSalarioBase.value) || 0) : 0;
        baseCC += (valBase * 2) / 12;
        const baseCP = baseCC;

        // 3. Trabajador
        const cuotaCC = baseCC * 0.0470;
        const cuotaMEI = baseCC * 0.0012;
        const cuotaDes = baseCP * 0.0155;
        const cuotaFP = baseCP * 0.0010;
        const cuotaIRPF = baseIRPF * (porcentajeIrpf / 100);

        const totalDeducciones = cuotaCC + cuotaMEI + cuotaDes + cuotaFP + cuotaIRPF;
        const liquido = totalDevengado - totalDeducciones;

        // 4. Empresa
        const cEmpCC = baseCC * 0.2360;
        const cEmpAT = baseCP * 0.0150;
        const cEmpDes = baseCP * 0.0550;
        const cEmpForm = baseCP * 0.0060;
        const cEmpFog = baseCP * 0.0020;
        const cEmpMEI = baseCC * 0.0058;

        const totalEmpSS = cEmpCC + cEmpAT + cEmpDes + cEmpForm + cEmpFog + cEmpMEI;

        // 5. Pintar Trabajador
        document.getElementById('total_devengado').value = formatMoneda(totalDevengado);

        document.getElementById('base_cc').value = formatMoneda(baseCC);
        document.getElementById('base_mei').value = formatMoneda(baseCC);
        document.getElementById('base_desempleo').value = formatMoneda(baseCP);
        document.getElementById('base_fp').value = formatMoneda(baseCP);
        document.getElementById('base_irpf').value = formatMoneda(baseIRPF);

        document.getElementById('importe_cc').value = formatMoneda(cuotaCC);
        document.getElementById('importe_mei').value = formatMoneda(cuotaMEI);
        document.getElementById('importe_desempleo').value = formatMoneda(cuotaDes);
        document.getElementById('importe_fp').value = formatMoneda(cuotaFP);
        document.getElementById('importe_irpf').value = formatMoneda(cuotaIRPF);

        document.getElementById('total_aportaciones').value = formatMoneda(cuotaCC + cuotaMEI + cuotaDes + cuotaFP);
        document.getElementById('total_deducciones').value = formatMoneda(totalDeducciones);
        document.getElementById('liquido_percibir').value = formatMoneda(liquido);

        // 6. Pintar Empresa
        document.getElementById('base_empresa_cc').value = formatMoneda(baseCC);
        document.getElementById('base_empresa_at').value = formatMoneda(baseCP);
        document.getElementById('base_empresa_desempleo').value = formatMoneda(baseCP);
        document.getElementById('base_empresa_otros').value = formatMoneda(baseCP);

        document.getElementById('importe_empresa_cc').value = formatMoneda(cEmpCC);
        document.getElementById('importe_empresa_at').value = formatMoneda(cEmpAT);
        document.getElementById('importe_empresa_desempleo').value = formatMoneda(cEmpDes);
        document.getElementById('importe_empresa_otros').value = formatMoneda(cEmpForm + cEmpFog + cEmpMEI);

        // Asignar al input oculto para que lo reciba el Controller
        document.getElementById('total_empresa_ss_hidden').value = formatMoneda(totalEmpSS);

        // Suma Matemática Correcta: Salario Bruto + Seguridad Social Empresa
        const costeTotalCalculado = totalDevengado + totalEmpSS;
        document.getElementById('coste_total_empresa').value = formatMoneda(costeTotalCalculado);

        // 7. Hiddens numéricos puros (para C#)
        document.getElementById('importe_empresa_formacion').value = cEmpForm.toFixed(2);
        document.getElementById('importe_empresa_fogasa').value = cEmpFog.toFixed(2);
        document.getElementById('importe_empresa_mei').value = cEmpMEI.toFixed(2);
    }
});