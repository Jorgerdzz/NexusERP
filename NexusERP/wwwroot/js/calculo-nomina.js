/* --- wwwroot/js/calculo-nomina.js --- */

document.addEventListener('DOMContentLoaded', function () {

    const inputsDevengos = document.querySelectorAll('.input-devengo');
    const inputSalarioBase = document.querySelector('input[name="SalarioBase"]');
    const porcentajeIrpf = parseFloat(document.getElementById('porcentajeIrpfAplicado').value) || 0;
    const salarioSugeridoText = document.getElementById('salarioSugerido');

    if (inputSalarioBase && salarioSugeridoText) {
        let textoSugerido = salarioSugeridoText.innerText;
        let sugeridoStr = textoSugerido.replace(/\./g, '').replace(/\s/g, '').replace(',', '.');
        let sugeridoVal = parseFloat(sugeridoStr) || 0;

        if (inputSalarioBase.value === "" || inputSalarioBase.value === "0" || inputSalarioBase.value === "0.00" || inputSalarioBase.value === "0,00") {
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
        let horasExtras = 0;

        // 1. Sumar devengos
        inputsDevengos.forEach(input => {
            const importe = parseFloat(input.value) || 0;
            totalDevengado += importe;

            if (input.dataset.tributaIrpf === "true") {
                baseIRPF += importe;
                if (input.name === "HorasExtraordinarias") {
                    horasExtras += importe;
                } else {
                    baseCC += importe;
                }
            }
        });

        // 2. Prorrateo 2 pagas extra
        const valBase = inputSalarioBase ? (parseFloat(inputSalarioBase.value) || 0) : 0;
        const prorrateo = (valBase * 2) / 12;
        baseCC += prorrateo;

        const baseCP = baseCC + horasExtras;

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
        const cEmpMEI = baseCC * 0.0058;

        const cEmpAT = baseCP * 0.0150;
        const cEmpDes = baseCP * 0.0550;
        const cEmpForm = baseCP * 0.0060;
        const cEmpFog = baseCP * 0.0020;

        const cEmpHorasExtras = horasExtras * 0.2360;

        const totalEmpSS = cEmpCC + cEmpMEI + cEmpAT + cEmpDes + cEmpForm + cEmpFog + cEmpHorasExtras;

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
        document.getElementById('base_empresa_he').value = formatMoneda(horasExtras); 
        document.getElementById('base_empresa_at').value = formatMoneda(baseCP);
        document.getElementById('base_empresa_desempleo').value = formatMoneda(baseCP);
        document.getElementById('base_empresa_formacion').value = formatMoneda(baseCP);
        document.getElementById('base_empresa_fogasa').value = formatMoneda(baseCP);
        document.getElementById('base_empresa_mei').value = formatMoneda(baseCC); 

        // Asignamos los importes calculados
        document.getElementById('importe_empresa_cc').value = formatMoneda(cEmpCC);
        document.getElementById('importe_empresa_he').value = formatMoneda(cEmpHorasExtras);
        document.getElementById('importe_empresa_at').value = formatMoneda(cEmpAT);
        document.getElementById('importe_empresa_desempleo').value = formatMoneda(cEmpDes);
        document.getElementById('importe_empresa_formacion').value = formatMoneda(cEmpForm);
        document.getElementById('importe_empresa_fogasa').value = formatMoneda(cEmpFog);
        document.getElementById('importe_empresa_mei').value = formatMoneda(cEmpMEI);

        // Asignar al input oculto para que lo reciba el Controller
        document.getElementById('total_empresa_ss_hidden').value = formatMoneda(totalEmpSS);

        // Suma Matemática Correcta: Salario Bruto + Seguridad Social Empresa
        const costeTotalCalculado = totalDevengado + totalEmpSS;
        document.getElementById('coste_total_empresa').value = formatMoneda(costeTotalCalculado);
    }
});