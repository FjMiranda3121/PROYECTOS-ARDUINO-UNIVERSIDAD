// 1. SEGURIDAD, AUTENTICACIÓN Y ROL (OPERADOR)
const token = localStorage.getItem('ecoRutas_jwt');

if (!token) {
    window.location.replace('login.html');
}

let emailOperador = '@operador';

try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const rol = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
             || payload.role 
             || payload['role'];
             
    emailOperador = payload.email 
        || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] 
        || payload['sub'] 
        || '@operador';

    // Si un administrador ingresa aquí, lo redirigimos a su Dashboard
    if (rol === 'ADMIN') {
        window.location.replace('dashboard.html');
    } else if (!rol) {
        localStorage.removeItem('ecoRutas_jwt');
        window.location.replace('login.html');
    }
} catch (e) {
    console.error("Error al decodificar JWT:", e);
    localStorage.removeItem('ecoRutas_jwt');
    window.location.replace('login.html');
}

// 2. CONFIGURACIÓN GLOBAL Y ESTADO DE CONEXIÓN
const API_BASE_URL = 'https://ecorutas-api.onrender.com/api/v3';
let canecaAConfirmarId = null;

function actualizarEstadoApi(isOnline) {
    const container = document.getElementById('apiStatusContainer');
    const ping = document.getElementById('apiStatusPing');
    const dot = document.getElementById('apiStatusDot');
    const text = document.getElementById('apiStatusText');

    if (!container || !dot || !text) return;

    if (isOnline) {
        container.className = 'flex items-center gap-1.5 bg-green-950/80 px-2.5 py-1 rounded-full border border-green-800 transition-colors';
        if (ping) ping.className = 'animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75';
        dot.className = 'relative inline-flex rounded-full h-2 w-2 bg-green-500';
        text.className = 'text-[10px] font-bold text-green-300';
        text.textContent = 'Online';
    } else {
        container.className = 'flex items-center gap-1.5 bg-red-950/80 px-2.5 py-1 rounded-full border border-red-800 transition-colors';
        if (ping) ping.className = 'hidden';
        dot.className = 'relative inline-flex rounded-full h-2 w-2 bg-red-500';
        text.className = 'text-[10px] font-bold text-red-300';
        text.textContent = 'Offline';
    }
}

// 3. CARGA DE RUTA PENDIENTE (RECOLECCIONES)
async function cargarRutaOperador() {
    try {
        // Petición exclusiva al controlador de recolecciones autorizadas para el operador
        const respuesta = await fetch(`${API_BASE_URL}/recolecciones/pendientes`, {
            method: 'GET',
            headers: { 
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (!respuesta.ok) {
            actualizarEstadoApi(false);
            
            if (respuesta.status === 401) {
                localStorage.removeItem('ecoRutas_jwt');
                window.location.replace('login.html');
                return;
            }
            
            throw new Error(`Error en servidor: HTTP ${respuesta.status}`);
        }

        const rawData = await respuesta.json();
        actualizarEstadoApi(true);

        const pendientes = Array.isArray(rawData) ? rawData : (rawData.datos || rawData.$values || []);
        renderizarCanecasOperador(pendientes);

    } catch (error) {
        console.error("Error al obtener la ruta del operador:", error);
        actualizarEstadoApi(false);
    }
}

// 4. RENDERIZADO DE TARJETAS MÓVILES
function renderizarCanecasOperador(canecas) {
    const contenedor = document.getElementById('contenedorRutaOperador');
    const cantPendientes = document.getElementById('cantPendientes');
    if (!contenedor) return;

    contenedor.innerHTML = '';

    if (!Array.isArray(canecas) || canecas.length === 0) {
        if (cantPendientes) cantPendientes.textContent = '0';
        contenedor.innerHTML = `
            <div class="p-8 text-center text-slate-400 bg-white rounded-2xl border border-slate-200 shadow-sm">
                <i class="fa-solid fa-circle-check text-emerald-500 text-3xl mb-2"></i>
                <p class="font-bold text-slate-700 text-sm">¡Ruta al día!</p>
                <p class="text-xs text-slate-500 mt-1">No hay puntos requeridos por recolección en este momento.</p>
            </div>`;
        return;
    }

    if (cantPendientes) cantPendientes.textContent = canecas.length.toString();

    // Ordenamiento por nivel de llenado descendente
    canecas.sort((a, b) => {
        const pA = a.porcentajeEstimado ?? a.porcentajellenado ?? a.porcentajeLlenado ?? 0;
        const pB = b.porcentajeEstimado ?? b.porcentajellenado ?? b.porcentajeLlenado ?? 0;
        return pB - pA;
    });

    canecas.forEach(c => {
        const id = c.canecaId || c.id || c.Id || '-';
        const porcentaje = c.porcentajeEstimado ?? c.porcentajellenado ?? c.porcentajeLlenado ?? 0;
        let estado = c.estado || c.estadoClasificado || c.estadoActual || 'NORMAL';

        if (porcentaje >= 90) estado = 'CRITICO';
        else if (porcentaje >= 70) estado = 'ATENCION';

        let colorCard = 'border-slate-200 bg-white';
        let colorBadge = 'bg-green-100 text-green-800 border-green-300';
        let colorBarra = 'bg-green-500';

        if (estado === 'ATENCION') {
            colorCard = 'border-amber-200 bg-amber-50/40';
            colorBadge = 'bg-amber-100 text-amber-800 border-amber-300';
            colorBarra = 'bg-amber-500';
        } else if (estado === 'CRITICO') {
            colorCard = 'border-red-200 bg-red-50/40';
            colorBadge = 'bg-red-100 text-red-800 border-red-300';
            colorBarra = 'bg-red-600';
        }

        const lat = c.latitud || 10.39972;
        const lng = c.longitud || -75.51444;

        const htmlCard = `
            <div class="p-4 ${colorCard} border rounded-2xl shadow-sm space-y-3 transition-all hover:shadow-md">
                <div class="flex justify-between items-start">
                    <div>
                        <span class="font-black text-slate-800 text-base">${id}</span>
                        <p class="text-xs text-slate-500 font-medium"><i class="fa-solid fa-location-dot mr-1 text-emerald-600"></i>${c.ubicacion || 'Punto de Recolección'}</p>
                    </div>
                    <span class="px-2.5 py-0.5 text-[10px] font-black rounded-full border ${colorBadge}">
                        ${estado}
                    </span>
                </div>

                <div>
                    <div class="flex justify-between text-xs font-bold text-slate-700 mb-1">
                        <span>Nivel actual:</span>
                        <span>${porcentaje}%</span>
                    </div>
                    <div class="w-full bg-slate-200/80 rounded-full h-2.5 overflow-hidden border border-slate-300/40">
                        <div class="${colorBarra} h-2.5 rounded-full transition-all duration-500" style="width: ${porcentaje}%"></div>
                    </div>
                </div>

                <div class="flex gap-2 pt-1">
                    <a href="https://www.google.com/maps/search/?api=1&query=${lat},${lng}" target="_blank" 
                       class="flex-1 bg-slate-100 hover:bg-slate-200 text-slate-700 font-bold py-2.5 rounded-xl text-xs flex items-center justify-center gap-1.5 transition-colors">
                        <i class="fa-solid fa-location-arrow text-blue-600"></i>
                        <span>Navegar</span>
                    </a>
                    <button onclick="abrirModalRecoleccion('${id}')" 
                            class="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-2.5 rounded-xl text-xs flex items-center justify-center gap-1.5 transition-colors shadow-sm">
                        <i class="fa-solid fa-check"></i>
                        <span>Vaciar</span>
                    </button>
                </div>
            </div>
        `;
        contenedor.insertAdjacentHTML('beforeend', htmlCard);
    });
}

// 5. MODAL TÁCTIL DE VACIADO
const modalConfirmar = document.getElementById('modalConfirmarRecoleccion');
const btnCancelar = document.getElementById('btnCancelarRecoleccion');
const btnConfirmar = document.getElementById('btnConfirmarRecoleccion');

window.abrirModalRecoleccion = function(id) {
    canecaAConfirmarId = id;
    const textDisplay = document.getElementById('modalCanecaIdText');
    if (textDisplay) textDisplay.textContent = id;
    if (modalConfirmar) modalConfirmar.classList.remove('hidden');
};

if (btnCancelar) {
    btnCancelar.addEventListener('click', () => {
        if (modalConfirmar) modalConfirmar.classList.add('hidden');
        canecaAConfirmarId = null;
    });
}

if (btnConfirmar) {
    btnConfirmar.addEventListener('click', async () => {
        if (!canecaAConfirmarId) return;

        btnConfirmar.disabled = true;
        btnConfirmar.innerHTML = `<i class="fa-solid fa-spinner animate-spin"></i> Registrando...`;

        try {
            const respuesta = await fetch(`${API_BASE_URL}/canecas/${canecaAConfirmarId}/vaciar`, {
                method: 'POST',
                headers: { 
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (respuesta.ok || respuesta.status === 204) {
                if (modalConfirmar) modalConfirmar.classList.add('hidden');
                cargarRutaOperador();
            } else {
                const respFallback = await fetch(`${API_BASE_URL}/recolecciones`, {
                    method: 'POST',
                    headers: { 
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ canecaId: canecaAConfirmarId })
                });

                if (respFallback.ok) {
                    if (modalConfirmar) modalConfirmar.classList.add('hidden');
                    cargarRutaOperador();
                } else {
                    alert('No se pudo confirmar la recolección.');
                }
            }
        } catch (error) {
            console.error("Error al registrar recolección:", error);
            alert('Error de conexión al intentar vaciar el contenedor.');
        } finally {
            btnConfirmar.disabled = false;
            btnConfirmar.innerHTML = `<i class="fa-solid fa-check"></i><span>Confirmar</span>`;
            canecaAConfirmarId = null;
        }
    });
}

// 6. INICIALIZACIÓN Y EVENTOS
document.addEventListener('DOMContentLoaded', () => {
    const userDisplay = document.getElementById('userNameDisplay');
    if (userDisplay) userDisplay.textContent = emailOperador;

    cargarRutaOperador();

    // Auto-refresco de sincronización cada 5 segundos
    setInterval(cargarRutaOperador, 5000);

    const btnLogout = document.getElementById('btnLogout');
    if (btnLogout) {
        btnLogout.addEventListener('click', () => {
            localStorage.removeItem('ecoRutas_jwt');
            window.location.replace('login.html');
        });
    }

    const btnActualizar = document.getElementById('btnActualizarOperador');
    if (btnActualizar) {
        btnActualizar.addEventListener('click', cargarRutaOperador);
    }
});