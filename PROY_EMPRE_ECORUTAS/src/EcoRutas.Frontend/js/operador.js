// 1. SEGURIDAD Y DECODIFICACIÓN DE JWT
const token = localStorage.getItem('ecoRutas_jwt');

if (!token) {
    window.location.replace('login.html');
}

// Extraer el correo del operador desde el payload del JWT
try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const emailOperador = payload.email 
        || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] 
        || payload['sub'] 
        || 'Operador Campo';
    
    document.addEventListener('DOMContentLoaded', () => {
        const userDisplay = document.getElementById('operadorEmail');
        if (userDisplay) userDisplay.textContent = emailOperador;
    });
} catch (e) {
    console.warn("No se pudo decodificar el token JWT:", e);
}

// 2. CONFIGURACIÓN GLOBAL
const API_BASE_URL = 'https://ecorutas-api.onrender.com/api/v3';

// CONTROLADOR VISUAL DE ESTADO API (ONLINE / OFFLINE)
function actualizarEstadoApi(isOnline) {
    const container = document.getElementById('apiStatusContainer');
    const ping = document.getElementById('apiStatusPing');
    const dot = document.getElementById('apiStatusDot');
    const text = document.getElementById('apiStatusText');

    if (!container || !dot || !text) return;

    if (isOnline) {
        container.className = 'flex items-center gap-1.5 bg-green-500/20 px-2.5 py-1 rounded-full border border-green-400/30 transition-colors';
        if (ping) ping.className = 'animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75';
        dot.className = 'relative inline-flex rounded-full h-2.5 w-2.5 bg-green-400';
        text.className = 'text-[10px] font-black text-green-300 uppercase tracking-wider';
        text.textContent = 'Online';
    } else {
        container.className = 'flex items-center gap-1.5 bg-red-500/20 px-2.5 py-1 rounded-full border border-red-400/30 transition-colors';
        if (ping) ping.className = 'hidden'; // Ocultar animación si está offline
        dot.className = 'relative inline-flex rounded-full h-2.5 w-2.5 bg-red-500';
        text.className = 'text-[10px] font-black text-red-300 uppercase tracking-wider';
        text.textContent = 'Offline';
    }
}
// 3. CONSULTAR RUTAS PENDIENTES (GET /recolecciones/pendientes)
async function cargarRutasPriorizadas() {
    const contenedorLista = document.getElementById('listaRutasContainer');

    try {
        const respuesta = await fetch(`${API_BASE_URL}/recolecciones/pendientes`, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!respuesta.ok) {
            actualizarEstadoApi(false); // Servidor responde con error HTTP
            if (respuesta.status === 401 || respuesta.status === 403) {
                localStorage.removeItem('ecoRutas_jwt');
                window.location.replace('login.html');
                return;
            }
            throw new Error(`Error HTTP: ${respuesta.status}`);
        }

        const rutasPendientes = await respuesta.json();
        actualizarEstadoApi(true); // Conexión exitosa -> API Online
        renderizarRutas(rutasPendientes);

    } catch (error) {
        console.error("Error al obtener la ruta pendiente:", error);
        actualizarEstadoApi(false); // Caída de red o timeout en Render -> API Offline
        if (contenedorLista) {
            contenedorLista.innerHTML = `
                <div class="bg-red-50 border border-red-200 text-red-700 p-4 rounded-xl text-center text-sm font-semibold">
                    Error de conexión con la central en Render.
                </div>`;
        }
    }
}

function renderizarRutas(rutas) {
    const contenedorLista = document.getElementById('listaRutasContainer');
    const cantPendientes = document.getElementById('cantPendientes');
    
    if (!contenedorLista) return;
    
    contenedorLista.innerHTML = '';
    if (cantPendientes) cantPendientes.textContent = rutas.length;

    if (rutas.length === 0) {
        contenedorLista.innerHTML = `
            <div class="bg-green-50 border border-green-200 p-6 rounded-2xl text-center">
                <i class="fa-solid fa-circle-check text-green-500 text-4xl mb-2"></i>
                <h3 class="font-bold text-green-800 text-base">¡Ruta Limpia!</h3>
                <p class="text-xs text-green-600 mt-1">No hay contenedores críticos ni en atención actualmente.</p>
            </div>`;
        return;
    }

    rutas.forEach(item => {
        const porcentaje = item.porcentajeLlenado ?? 0;
        const estado = item.estadoActual || 'CRITICO';

        let colorBg = 'bg-red-50';
        let colorBorder = 'border-red-200';
        let colorBadge = 'bg-red-600 text-white';

        if (estado === 'ATENCION') {
            colorBg = 'bg-yellow-50';
            colorBorder = 'border-yellow-200';
            colorBadge = 'bg-yellow-500 text-white';
        }

        const htmlItem = `
            <div class="${colorBg} border ${colorBorder} p-4 rounded-2xl shadow-sm space-y-3">
                <div class="flex justify-between items-center">
                    <div>
                        <span class="font-black text-slate-800 text-lg">${item.id}</span>
                        <p class="text-xs text-slate-500 font-medium">
                            <i class="fa-solid fa-location-dot mr-1"></i>${item.ubicacion || 'Sin ubicación'}
                        </p>
                    </div>
                    <span class="${colorBadge} text-xs font-black px-3 py-1 rounded-full uppercase tracking-wider">
                        ${estado} (${porcentaje}%)
                    </span>
                </div>

                ${item.latitud && item.longitud ? `
                    <a href="https://www.google.com/maps/search/?api=1&query=${item.latitud},${item.longitud}" target="_blank" class="flex items-center justify-center gap-2 w-full bg-white border border-slate-300 text-slate-700 font-semibold py-2 rounded-xl text-xs shadow-sm hover:bg-slate-50 transition-colors">
                        <i class="fa-solid fa-map-location-dot text-blue-600"></i> Cómo llegar (Navegar)
                    </a>
                ` : ''}

                <button onclick="confirmarRecoleccion('${item.id}')" class="w-full bg-slate-900 hover:bg-black text-white font-bold py-3 rounded-xl shadow-md transition-colors flex items-center justify-center gap-2 text-sm">
                    <i class="fa-solid fa-check"></i> Confirmar Recolección
                </button>
            </div>
        `;
        contenedorLista.insertAdjacentHTML('beforeend', htmlItem);
    });
}

// 4. CONFIRMAR RECOLECCIÓN (MODAL CUSTOM - POST /recolecciones)
let canecaAConfirmar = null;

const modalRecoleccion = document.getElementById('modalConfirmarRecoleccion');
const btnCancelarRecoleccion = document.getElementById('btnCancelarRecoleccion');
const btnEjecutarRecoleccion = document.getElementById('btnEjecutarRecoleccion');

// Disparador desde el botón de la tarjeta
window.confirmarRecoleccion = function(canecaId) {
    canecaAConfirmar = canecaId;
    const textDisplay = document.getElementById('idCanecaRecoleccionText');
    if (textDisplay) textDisplay.textContent = canecaId;
    if (modalRecoleccion) modalRecoleccion.classList.remove('hidden');
};

// Evento: Cancelar o cerrar modal
if (btnCancelarRecoleccion) {
    btnCancelarRecoleccion.addEventListener('click', () => {
        if (modalRecoleccion) modalRecoleccion.classList.add('hidden');
        canecaAConfirmar = null;
    });
}

// Evento: Confirmar y enviar POST a la API
if (btnEjecutarRecoleccion) {
    btnEjecutarRecoleccion.addEventListener('click', async () => {
        if (!canecaAConfirmar) return;

        btnEjecutarRecoleccion.textContent = 'Guardando...';
        btnEjecutarRecoleccion.disabled = true;

        try {
            const respuesta = await fetch(`${API_BASE_URL}/recolecciones`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ canecaId: canecaAConfirmar })
            });

            if (respuesta.ok) {
                if (modalRecoleccion) modalRecoleccion.classList.add('hidden');
                cargarRutasPriorizadas(); // Recargar la ruta en vivo
            } else {
                const errorData = await respuesta.json().catch(() => ({}));
                alert(`Error al registrar recolección: ${errorData.title || errorData.message || 'Intente nuevamente.'}`);
            }
        } catch (error) {
            console.error("Error en recolección:", error);
            alert('Error de conexión al registrar la recolección.');
        } finally {
            btnEjecutarRecoleccion.innerHTML = '<i class="fa-solid fa-check"></i> Vaciar';
            btnEjecutarRecoleccion.disabled = false;
            canecaAConfirmar = null;
        }
    });
}

// 5. EVENTOS Y ARRANQUE INICIAL (PUNTO 6: AUTO-REFRESH)
document.addEventListener('DOMContentLoaded', () => {
    const btnLogout = document.getElementById('btnLogoutOperador');
    if (btnLogout) {
        btnLogout.addEventListener('click', () => {
            localStorage.removeItem('ecoRutas_jwt');
            window.location.replace('login.html');
        });
    }

    const btnActualizar = document.getElementById('btnActualizarRuta');
    if (btnActualizar) {
        btnActualizar.addEventListener('click', cargarRutasPriorizadas);
    }

    // 1. Carga inicial al abrir la aplicación
    cargarRutasPriorizadas();

    // 2. PUNTO 6: Auto-refresh periódico cada 8 segundos (8000 ms)
    setInterval(cargarRutasPriorizadas, 8000);
});