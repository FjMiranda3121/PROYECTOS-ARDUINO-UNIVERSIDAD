// CONTROLADOR DE AUTENTICACIÓN Y LOGIN
const API_BASE_URL = 'https://ecorutas-api.onrender.com/api/v3';

document.addEventListener('DOMContentLoaded', () => {
    // 1. Si ya existe un token activo, redirigir según el rol
    const tokenExistente = localStorage.getItem('ecoRutas_jwt');
    if (tokenExistente) {
        try {
            const payload = JSON.parse(atob(tokenExistente.split('.')[1]));
            const rol = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
                     || payload.role 
                     || payload['role'];
            
            if (rol === 'OPERADOR') {
                window.location.replace('operador.html');
            } else {
                window.location.replace('dashboard.html');
            }
            return;
        } catch (e) {
            localStorage.removeItem('ecoRutas_jwt');
        }
    }

    const formLogin = document.getElementById('formLogin');
    const emailInput = document.getElementById('emailInput');
    const passwordInput = document.getElementById('passwordInput');
    const btnLoginSubmit = document.getElementById('btnLoginSubmit');
    const errorLogin = document.getElementById('errorLogin');

    if (!formLogin) return;

    // 2. Evento de Envío del Formulario
    formLogin.addEventListener('submit', async (e) => {
        e.preventDefault();

        const email = emailInput.value.trim();
        const password = passwordInput.value;

        if (errorLogin) errorLogin.classList.add('hidden');

        // Estado visual de carga en el botón
        const textoOriginalBoton = btnLoginSubmit.innerHTML;
        btnLoginSubmit.disabled = true;
        btnLoginSubmit.innerHTML = `<i class="fa-solid fa-spinner animate-spin"></i> Conectando...`;

        try {
            const respuesta = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email, password })
            });

            if (!respuesta.ok) {
                const errorData = await respuesta.json().catch(() => ({}));
                throw new Error(errorData.detail || errorData.message || 'Credenciales incorrectas');
            }

            const data = await respuesta.json();
            const token = data.token || data.jwt || data;

            if (!token || typeof token !== 'string') {
                throw new Error('Respuesta de autenticación no válida.');
            }

            // Guardar token JWT en almacenamiento local
            localStorage.setItem('ecoRutas_jwt', token);

            // Decodificar JWT para verificar rol de acceso
            const payload = JSON.parse(atob(token.split('.')[1]));
            const rol = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
                     || payload.role 
                     || payload['role'];

            // Redirección según ROL asignado
            if (rol === 'OPERADOR') {
                window.location.replace('operador.html');
            } else {
                window.location.replace('dashboard.html');
            }

        } catch (error) {
            console.error("Error al iniciar sesión:", error);
            if (errorLogin) {
                errorLogin.textContent = error.message || 'Credenciales incorrectas o servidor no disponible.';
                errorLogin.classList.remove('hidden');
            }
        } finally {
            btnLoginSubmit.disabled = false;
            btnLoginSubmit.innerHTML = textoOriginalBoton;
        }
    });
});