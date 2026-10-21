// 1. SEGURIDAD, AUTENTICACIÓN Y ROL (ADMIN)
const token = localStorage.getItem('ecoRutas_jwt');

if (!token) {
    window.location.replace('login.html');
}

// Validar que el usuario tenga rol de ADMIN
try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const rol = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
             || payload.role 
             || payload['role'];
             
    const emailAdmin = payload.email 
        || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] 
        || payload['sub'] 
        || '@admin';

    // Protección estricta de ruta: Si es OPERADOR se envía a su portal
    if (rol !== 'ADMIN') {
        if (rol === 'OPERADOR') {
            window.location.replace('operador.html');
        } else {
            localStorage.removeItem('ecoRutas_jwt');
            window.location.replace('login.html');
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        const userDisplay = document.getElementById('userNameDisplay');
        if (userDisplay) userDisplay.textContent = emailAdmin;
    });

} catch (e) {
    console.error("Error al decodificar JWT:", e);
    localStorage.removeItem('ecoRutas_jwt');
    window.location.replace('login.html');
}

// 2. CONFIGURACIÓN GLOBAL Y MAPA PRINCIPAL
const API_BASE_URL = 'https://ecorutas-api.onrender.com/api/v3';
const CENTRO_CARTAGENA = [10.39079, -75.49664];
const ZOOM_INICIAL = 18.4;

let mapaPrincipal = null;
let marcadoresMap = {};
let canecaSeleccionadaId = null;

// Inicialización del Mapa de Leaflet (Coordenadas Cartagena)
function inicializarMapaPrincipal() {
    if (mapaPrincipal) return;
    
    mapaPrincipal = L.map('mapaPrincipal').setView(CENTRO_CARTAGENA, ZOOM_INICIAL);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap / EcoRutas v3.0'
    }).addTo(mapaPrincipal);
}

// 3. CAMBIO DE VISTAS Y MENÚ LATERAL DESPLEGABLE
window.cambiarVista = function(vista) {
    const vistaMapa = document.getElementById('vistaMapa');
    const vistaMediciones = document.getElementById('vistaMediciones');
    const navBtnMapa = document.getElementById('navBtnMapa');
    const navBtnMediciones = document.getElementById('navBtnMediciones');
    const tituloSeccion = document.getElementById('tituloSeccion');

    if (vista === 'mapa') {
        vistaMapa.classList.remove('hidden');
        vistaMediciones.classList.add('hidden');

        navBtnMapa.className = "nav-item active w-full flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all bg-emerald-600 text-white shadow-md";
        navBtnMediciones.className = "nav-item w-full flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all text-slate-400 hover:bg-slate-800 hover:text-white";
        
        if (tituloSeccion) tituloSeccion.textContent = "Monitoreo en Vivo";
        if (mapaPrincipal) setTimeout(() => mapaPrincipal.invalidateSize(), 200);

    } else if (vista === 'mediciones') {
        vistaMapa.classList.add('hidden');
        vistaMediciones.classList.remove('hidden');

        navBtnMediciones.className = "nav-item active w-full flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all bg-emerald-600 text-white shadow-md";
        navBtnMapa.className = "nav-item w-full flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all text-slate-400 hover:bg-slate-800 hover:text-white";

        if (tituloSeccion) tituloSeccion.textContent = "Telemetría y Mediciones";
        cargarMedicionesRecientes();
    }
};

// CONTROLADOR VISUAL DE ESTADO API (ONLINE / OFFLINE)
function actualizarEstadoApi(isOnline) {
    const container = document.getElementById('apiStatusContainer');
    const ping = document.getElementById('apiStatusPing');
    const dot = document.getElementById('apiStatusDot');
    const text = document.getElementById('apiStatusText');

    if (!container || !dot || !text) return;

    if (isOnline) {
        container.className = 'flex items-center gap-2 bg-green-50 px-3 py-1.5 rounded-md border border-green-200 transition-colors';
        if (ping) ping.className = 'animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75';
        dot.className = 'relative inline-flex rounded-full h-3 w-3 bg-green-500';
        text.className = 'text-xs font-bold text-green-700';
        text.textContent = 'API: Online';
    } else {
        container.className = 'flex items-center gap-2 bg-red-50 px-3 py-1.5 rounded-md border border-red-200 transition-colors';
        if (ping) ping.className = 'hidden';
        dot.className = 'relative inline-flex rounded-full h-3 w-3 bg-red-500';
        text.className = 'text-xs font-bold text-red-700';
        text.textContent = 'API: Offline';
    }
}

// 4. RENDERING Y LÓGICA DE CANECAS (GET /canecas)
function crearIconoMapa(estado) {
    let colorBg = 'bg-green-500';
    let animacion = '';

    if (estado === 'ATENCION') {
        colorBg = 'bg-amber-500';
    } else if (estado === 'CRITICO') {
        colorBg = 'bg-red-600';
        animacion = 'animate-bounce';
    }

    return L.divIcon({
        className: 'custom-map-pin',
        html: `<div class="w-8 h-8 ${colorBg} ${animacion} text-white rounded-full flex items-center justify-center shadow-lg border-2 border-white text-xs">
                <i class="fa-solid fa-trash"></i>
               </div>`,
        iconSize: [32, 32],
        iconAnchor: [16, 16],
        popupAnchor: [0, -16]
    });
}

async function cargarDatosCanecas() {
    try {
        const respuesta = await fetch(`${API_BASE_URL}/canecas?soloActivas=true`, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!respuesta.ok) {
            actualizarEstadoApi(false);
            if (respuesta.status === 401 || respuesta.status === 403) {
                localStorage.removeItem('ecoRutas_jwt');
                window.location.replace('login.html');
                return;
            }
            throw new Error('Error al obtener datos.');
        }

        const canecas = await respuesta.json();
        actualizarEstadoApi(true);
        
        renderizarKPIs(canecas);
        renderizarMapaYLista(canecas);

    } catch (error) {
        console.error("Error al cargar canecas:", error);
        actualizarEstadoApi(false);
    }
}

function renderizarKPIs(canecas) {
    const total = canecas.length;
    let normal = 0, atencion = 0, critico = 0;

    canecas.forEach(c => {
        // Mapeo flexible de porcentaje y estado segun C#
        const porcentaje = c.porcentajellenado ?? c.porcentajeLlenado ?? c.Porcentajellenado ?? 0;
        let estadoReal = c.estadoClasificado ?? c.estadoActual ?? 'NORMAL';

        if (porcentaje >= 90) estadoReal = 'CRITICO';
        else if (porcentaje >= 70) estadoReal = 'ATENCION';

        if (estadoReal === 'NORMAL') normal++;
        else if (estadoReal === 'ATENCION') atencion++;
        else if (estadoReal === 'CRITICO') critico++;
    });

    document.getElementById('kpi-total').textContent = total;
    document.getElementById('kpi-normal').textContent = normal;
    document.getElementById('kpi-atencion').textContent = atencion;
    document.getElementById('kpi-critico').textContent = critico;
}

// CONTROL DE SELECCIÓN Y TOGGLE EN MAPA
window.centrarEnMapa = function(id) {
    const marker = marcadoresMap[id];
    if (!marker || !mapaPrincipal) return;

    // CASO 1: Deseleccionar (clic por segunda vez)
    if (canecaSeleccionadaId === id) {
        marker.closePopup();
        mapaPrincipal.flyTo(CENTRO_CARTAGENA, ZOOM_INICIAL, {
            animate: true,
            duration: 1.2
        });
        canecaSeleccionadaId = null;
        quitarResaltadosTarjetas();
        return;
    }

    // CASO 2: Seleccionar nuevo contenedor
    const latLng = marker.getLatLng();
    mapaPrincipal.flyTo([latLng.lat, latLng.lng], 16, {
        animate: true,
        duration: 1.2
    });

    marker.openPopup();
    canecaSeleccionadaId = id;
    resaltarTarjeta(id);
};

function resaltarTarjeta(id) {
    quitarResaltadosTarjetas();
    const tarjeta = document.getElementById(`card-caneca-${id}`);
    if (tarjeta) {
        tarjeta.classList.add('ring-2', 'ring-emerald-500', 'shadow-md');
    }
}

function quitarResaltadosTarjetas() {
    document.querySelectorAll('.card-caneca-item').forEach(el => {
        el.classList.remove('ring-2', 'ring-emerald-500', 'shadow-md');
    });
}

function renderizarMapaYLista(canecas) {
    const contenedorLista = document.getElementById('listaCanecasContainer');
    if (!contenedorLista) return;

    contenedorLista.innerHTML = '';

    if (canecas.length === 0) {
        contenedorLista.innerHTML = '<p class="text-sm text-slate-400 text-center mt-4">No hay contenedores registrados.</p>';
        return;
    }

    canecas.forEach(caneca => {
        // Mapeo unificado con C# Medicion/Caneca
        const porcentaje = caneca.porcentajellenado ?? caneca.porcentajeLlenado ?? caneca.Porcentajellenado ?? 0;
        let estadoReal = caneca.estadoClasificado ?? caneca.estadoActual ?? 'NORMAL';

        if (porcentaje >= 90) {
            estadoReal = 'CRITICO';
        } else if (porcentaje >= 70) {
            estadoReal = 'ATENCION';
        }

        const isSelected = (caneca.id === canecaSeleccionadaId);
        const claseResaltado = isSelected ? 'ring-2 ring-emerald-500 shadow-md' : '';

        let colorCard = 'bg-green-50/80 border-green-300';
        let colorBadge = 'bg-green-100 text-green-800 border-green-400';
        let colorBarra = 'bg-green-500';

        if (estadoReal === 'ATENCION') {
            colorCard = 'bg-yellow-50/80 border-yellow-300';
            colorBadge = 'bg-yellow-100 text-yellow-800 border-yellow-400';
            colorBarra = 'bg-yellow-500';
        } else if (estadoReal === 'CRITICO') {
            colorCard = 'bg-red-50/80 border-red-300';
            colorBadge = 'bg-red-100 text-red-800 border-red-400';
            colorBarra = 'bg-red-600';
        }

        // 1. Agregar o actualizar Marcador Colorizado
        if (caneca.latitud && caneca.longitud) {
            const iconoEstado = crearIconoMapa(estadoReal);

            if (marcadoresMap[caneca.id]) {
                marcadoresMap[caneca.id].setLatLng([caneca.latitud, caneca.longitud]);
                marcadoresMap[caneca.id].setIcon(iconoEstado);
            } else {
                const marker = L.marker([caneca.latitud, caneca.longitud], { icon: iconoEstado }).addTo(mapaPrincipal);
                marker.bindPopup(`
                    <div class="text-center font-sans p-1">
                        <strong class="text-slate-800 text-sm">${caneca.id}</strong><br>
                        <span class="text-xs text-slate-500">${caneca.ubicacion}</span><br>
                        <span class="text-xs font-bold text-slate-800">Estado: ${estadoReal} (${porcentaje}%)</span>
                    </div>
                `);
                marcadoresMap[caneca.id] = marker;
            }
        }

        // 2. Renderizar Casilla Lateral
        const htmlCard = `
            <div id="card-caneca-${caneca.id}" 
                 class="card-caneca-item p-3 ${colorCard} border rounded-xl shadow-sm space-y-2 transition-all hover:shadow-md cursor-pointer ${claseResaltado}" 
                 onclick="centrarEnMapa('${caneca.id}')">
                <div class="flex justify-between items-center">
                    <div>
                        <span class="font-black text-slate-800 text-sm">${caneca.id}</span>
                        <p class="text-xs text-slate-600 font-medium"><i class="fa-solid fa-location-dot mr-1"></i>${caneca.ubicacion}</p>
                    </div>
                    
                    <div class="flex items-center gap-2">
                        <span class="px-2.5 py-0.5 text-[10px] font-black rounded-full border ${colorBadge}">
                            ${estadoReal}
                        </span>
                        
                        <!-- BOTÓN EDITAR -->
                        <button onclick="event.stopPropagation(); abrirModalEditar('${caneca.id}')" class="text-blue-600 hover:text-blue-800 transition-colors p-1" title="Editar Contenedor">
                            <i class="fa-solid fa-pen-to-square"></i>
                        </button>
                        
                        <!-- BOTÓN ELIMINAR -->
                        <button onclick="event.stopPropagation(); eliminarCaneca('${caneca.id}')" class="text-red-500 hover:text-red-700 transition-colors p-1" title="Eliminar Contenedor">
                            <i class="fa-solid fa-trash-can"></i>
                        </button>
                    </div>
                </div>

                <!-- Barra Dinámica -->
                <div>
                    <div class="flex justify-between text-[11px] font-bold text-slate-700 mb-1">
                        <span>Nivel de Llenado:</span>
                        <span>${porcentaje}%</span>
                    </div>
                    <div class="w-full bg-slate-200/80 rounded-full h-2 overflow-hidden border border-slate-300/50">
                        <div class="${colorBarra} h-2 rounded-full transition-all duration-500" style="width: ${porcentaje}%"></div>
                    </div>
                </div>
            </div>
        `;
        contenedorLista.insertAdjacentHTML('beforeend', htmlCard);
    });
}

// 5. MÓDULO DE TELEMETRÍA (MAPEO EXACTO POSTMAN)
window.cargarMedicionesRecientes = async function() {
    const tbody = document.getElementById('tablaMedicionesBody');
    if (!tbody) return;

    tbody.innerHTML = `<tr><td colspan="5" class="p-4 text-center text-slate-400"><i class="fa-solid fa-spinner animate-spin mr-2"></i>Cargando lecturas desde Render...</td></tr>`;

    try {
        const respuesta = await fetch(`${API_BASE_URL}/mediciones`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!respuesta.ok) throw new Error(`HTTP Error Status: ${respuesta.status}`);

        const jsonRaw = await respuesta.json();

        // Extraer el arreglo de la propiedad "datos" que vimos en Postman
        const listaMediciones = jsonRaw.datos || jsonRaw.data || (Array.isArray(jsonRaw) ? jsonRaw : []);

        tbody.innerHTML = '';

        if (listaMediciones.length === 0) {
            tbody.innerHTML = `<tr><td colspan="5" class="p-4 text-center text-slate-400">No se encontraron lecturas en la base de datos.</td></tr>`;
            return;
        }

        // Dentro de la función renderizar en cargarMedicionesRecientes:
        listaMediciones.forEach(m => {
            const id = m.id || '-';
            const canecaId = m.canecaId || '-';
            // Ahora lee directamente la propiedad distanciaCm enviada por el nuevo DTO
            const distancia = m.distanciaCm ?? m.distanciaMedidaCm ?? m.distancia ?? 0;
            const porcentaje = m.porcentajeEstimado ?? m.porcentajellenado ?? 0;
            const estado = m.estado || m.estadoClasificado || 'NORMAL';
            const fechaRaw = m.fechaRegistro || m.fechaLectura;
            const fecha = fechaRaw ? new Date(fechaRaw).toLocaleString() : '-';

            let colorBadge = 'bg-green-100 text-green-800 border-green-300';
            if (estado === 'ATENCION') {
                colorBadge = 'bg-yellow-100 text-yellow-800 border-yellow-300';
            } else if (estado === 'CRITICO') {
                colorBadge = 'bg-red-100 text-red-800 border-red-300';
            }

            const htmlRow = `
                <tr class="hover:bg-slate-50 transition-colors">
                    <td class="p-3 font-mono text-[11px] font-bold text-slate-500 truncate max-w-[140px]" title="${id}">${id}</td>
                    <td class="p-3 font-black text-slate-800">${canecaId}</td>
                    <td class="p-3 text-center font-bold text-emerald-600">${distancia} cm</td>
                    <td class="p-3 text-center">
                        <span class="font-bold text-slate-700 mr-1.5">${porcentaje}%</span>
                        <span class="px-2 py-0.5 text-[10px] font-black rounded-full border ${colorBadge}">${estado}</span>
                    </td>
                    <td class="p-3 text-right text-slate-500 font-medium">${fecha}</td>
                </tr>
            `;
            tbody.insertAdjacentHTML('beforeend', htmlRow);
        });

    } catch (error) {
        console.error("Error al cargar mediciones:", error);
        tbody.innerHTML = `<tr><td colspan="5" class="p-4 text-center text-red-500 font-bold">Error al conectar con la API (${error.message}). Revisa la consola F12.</td></tr>`;
    }
};

// 6. CREAR NUEVA CANECA (POST /canecas)
let miniMapaFormulario = null;
let markerFormulario = null;

const modalAgregar = document.getElementById('modalAgregar');
const btnAbrirModal = document.getElementById('btnAbrirModal');
const btnCerrarModal = document.getElementById('btnCerrarModal');
const formNuevaCaneca = document.getElementById('formNuevaCaneca');

if (btnAbrirModal) {
    btnAbrirModal.addEventListener('click', () => {
        modalAgregar.classList.remove('hidden');
        
        setTimeout(() => {
            if (!miniMapaFormulario) {
                miniMapaFormulario = L.map('miniMapaFormulario').setView(CENTRO_CARTAGENA, ZOOM_INICIAL);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(miniMapaFormulario);

                miniMapaFormulario.on('click', (e) => {
                    const { lat, lng } = e.latlng;
                    document.getElementById('latForm').value = lat.toFixed(6);
                    document.getElementById('lngForm').value = lng.toFixed(6);

                    if (markerFormulario) miniMapaFormulario.removeLayer(markerFormulario);
                    markerFormulario = L.marker([lat, lng]).addTo(miniMapaFormulario);
                });
            } else {
                miniMapaFormulario.invalidateSize();
            }
        }, 200);
    });
}

if (btnCerrarModal) {
    btnCerrarModal.addEventListener('click', () => {
        modalAgregar.classList.add('hidden');
    });
}

if (formNuevaCaneca) {
    formNuevaCaneca.addEventListener('submit', async (e) => {
        e.preventDefault();

        const dto = {
            id: document.getElementById('nuevoCanecaId').value,
            ubicacion: document.getElementById('nuevaUbicacion').value,
            profundidadMaxCm: parseFloat(document.getElementById('nuevaProfundidad').value),
            distanciaCriticaCm: parseFloat(document.getElementById('nuevaDistanciaCritica').value),
            latitud: parseFloat(document.getElementById('latForm').value),
            longitud: parseFloat(document.getElementById('lngForm').value)
        };

        try {
            const respuesta = await fetch(`${API_BASE_URL}/canecas`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dto)
            });

            if (respuesta.ok) {
                modalAgregar.classList.add('hidden');
                formNuevaCaneca.reset();
                if (markerFormulario && miniMapaFormulario) miniMapaFormulario.removeLayer(markerFormulario);
                cargarDatosCanecas();
            } else {
                const err = await respuesta.json().catch(() => ({}));
                alert(`Error al crear: ${err.detail || 'Verifique los campos.'}`);
            }
        } catch (error) {
            console.error("Error al registrar caneca:", error);
            alert('Error de conexión con el servidor.');
        }
    });
}

// 7. EDITAR CANECA (PUT /canecas/{id})
let miniMapaEditar = null;
let markerEditar = null;

const modalEditar = document.getElementById('modalEditar');
const btnCerrarModalEditar = document.getElementById('btnCerrarModalEditar');
const formEditarCaneca = document.getElementById('formEditarCaneca');

window.abrirModalEditar = async function(id) {
    try {
        const respuesta = await fetch(`${API_BASE_URL}/canecas/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!respuesta.ok) throw new Error('No se pudieron obtener los datos.');

        const caneca = await respuesta.json();

        document.getElementById('editCanecaId').value = caneca.id;
        document.getElementById('editCanecaIdTitle').textContent = caneca.id;
        document.getElementById('editUbicacion').value = caneca.ubicacion;
        document.getElementById('editProfundidad').value = caneca.profundidadMaxCm;
        document.getElementById('editDistanciaCritica').value = caneca.distanciaCriticaCm;
        document.getElementById('editLatForm').value = caneca.latitud;
        document.getElementById('editLngForm').value = caneca.longitud;

        modalEditar.classList.remove('hidden');

        setTimeout(() => {
            if (!miniMapaEditar) {
                miniMapaEditar = L.map('miniMapaEditar').setView([caneca.latitud, caneca.longitud], 15);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(miniMapaEditar);

                miniMapaEditar.on('click', (e) => {
                    const { lat, lng } = e.latlng;
                    document.getElementById('editLatForm').value = lat.toFixed(6);
                    document.getElementById('editLngForm').value = lng.toFixed(6);

                    if (markerEditar) miniMapaEditar.removeLayer(markerEditar);
                    markerEditar = L.marker([lat, lng]).addTo(miniMapaEditar);
                });
            } else {
                miniMapaEditar.setView([caneca.latitud, caneca.longitud], 15);
                miniMapaEditar.invalidateSize();
            }

            if (markerEditar) miniMapaEditar.removeLayer(markerEditar);
            markerEditar = L.marker([caneca.latitud, caneca.longitud]).addTo(miniMapaEditar);
        }, 200);

    } catch (error) {
        console.error("Error al cargar datos para edición:", error);
        alert('Error al consultar el contenedor.');
    }
};

if (btnCerrarModalEditar) {
    btnCerrarModalEditar.addEventListener('click', () => {
        modalEditar.classList.add('hidden');
    });
}

if (formEditarCaneca) {
    formEditarCaneca.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('editCanecaId').value;
        const dto = {
            ubicacion: document.getElementById('editUbicacion').value,
            profundidadMaxCm: parseFloat(document.getElementById('editProfundidad').value),
            distanciaCriticaCm: parseFloat(document.getElementById('editDistanciaCritica').value),
            latitud: parseFloat(document.getElementById('editLatForm').value),
            longitud: parseFloat(document.getElementById('editLngForm').value)
        };

        try {
            const respuesta = await fetch(`${API_BASE_URL}/canecas/${id}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dto)
            });

            if (respuesta.ok) {
                modalEditar.classList.add('hidden');
                cargarDatosCanecas();
            } else {
                const err = await respuesta.json().catch(() => ({}));
                alert(`Error al actualizar: ${err.detail || 'Verifique los campos.'}`);
            }
        } catch (error) {
            console.error("Error al actualizar caneca:", error);
            alert('Error de conexión con el servidor.');
        }
    });
}

// 8. ELIMINAR CANECA (DELETE /canecas/{id})
let idCanecaAEliminar = null;

const modalEliminar = document.getElementById('modalEliminar');
const btnCancelarEliminar = document.getElementById('btnCancelarEliminar');
const btnConfirmarEliminar = document.getElementById('btnConfirmarEliminar');

window.eliminarCaneca = function(id) {
    idCanecaAEliminar = id;
    const textDisplay = document.getElementById('idCanecaEliminarText');
    if (textDisplay) textDisplay.textContent = id;
    if (modalEliminar) modalEliminar.classList.remove('hidden');
};

if (btnCancelarEliminar) {
    btnCancelarEliminar.addEventListener('click', () => {
        if (modalEliminar) modalEliminar.classList.add('hidden');
        idCanecaAEliminar = null;
    });
}

if (btnConfirmarEliminar) {
    btnConfirmarEliminar.addEventListener('click', async () => {
        if (!idCanecaAEliminar) return;

        btnConfirmarEliminar.textContent = 'Eliminando...';
        btnConfirmarEliminar.disabled = true;

        try {
            const respuesta = await fetch(`${API_BASE_URL}/canecas/${idCanecaAEliminar}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (respuesta.ok) {
                if (modalEliminar) modalEliminar.classList.add('hidden');
                
                if (marcadoresMap[idCanecaAEliminar]) {
                    mapaPrincipal.removeLayer(marcadoresMap[idCanecaAEliminar]);
                    delete marcadoresMap[idCanecaAEliminar];
                }

                cargarDatosCanecas();
            } else {
                alert('No se pudo eliminar el contenedor.');
            }
        } catch (error) {
            console.error("Error al eliminar:", error);
            alert('Error de conexión al intentar eliminar.');
        } finally {
            btnConfirmarEliminar.innerHTML = '<i class="fa-regular fa-trash-can"></i> Eliminar';
            btnConfirmarEliminar.disabled = false;
            idCanecaAEliminar = null;
        }
    });
}

// 9. INICIALIZACIÓN Y AUTO-REFRESH
document.addEventListener('DOMContentLoaded', () => {
    inicializarMapaPrincipal();
    cargarDatosCanecas();

    // Refréscate automático cada 5s para sincronizar mediciones del ESP32
    setInterval(cargarDatosCanecas, 5000);

    // Toggle de Menú Lateral
    const btnToggleSidebar = document.getElementById('btnToggleSidebar');
    const sidebarNav = document.getElementById('sidebarNav');

    if (btnToggleSidebar && sidebarNav) {
        btnToggleSidebar.addEventListener('click', () => {
            sidebarNav.classList.toggle('-ml-64');
            setTimeout(() => {
                if (mapaPrincipal) {
                    mapaPrincipal.invalidateSize();
                }
            }, 300);
        });
    }

    const btnLogout = document.getElementById('btnLogout');
    if (btnLogout) {
        btnLogout.addEventListener('click', () => {
            localStorage.removeItem('ecoRutas_jwt');
            window.location.replace('login.html');
        });
    }

    const btnActualizar = document.getElementById('btnActualizar');
    if (btnActualizar) {
        btnActualizar.addEventListener('click', cargarDatosCanecas);
    }
});