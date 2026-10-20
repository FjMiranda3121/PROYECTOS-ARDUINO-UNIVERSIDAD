document.addEventListener('DOMContentLoaded', function(){


    const formLogin = document.getElementById('formLogin');

    if(formLogin){
        formLogin.addEventListener('submit', async function name(e) {
        // 1. Evitar que la pagina se recargue al enviar el formulario
        e.preventDefault();
        //2. Capturar los valores de los inputs y elementos del DOM
        const emailInput = document.getElementById('email');
        const passwordInput = document.getElementById('password');
        const errorDiv = document.getElementById('errorMensaje');
        const btnSubmit = this.querySelector('button[type="submit"]');

            if (!errorDiv){
                console.error("Falta el <div id='errorMensaje'></div>");
                return;
            }
            const email = emailInput.value;
            const password = passwordInput.value;

        // Limpiar errores previos y dar feedback visual de carga
        errorDiv.classList.add('hidden');
        btnSubmit.textContent = 'Autenticado...';
        btnSubmit.disabled = true;

        
        //3. Hacer la petición HTTP POST 
        try {
            const response = await fetch('https://ecorutas-api.onrender.com/api/v3/auth/login',{
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    Email:email,
                    Password:password
                })
            });

            //4. Evaluar respuesta del servidor
            if (response.ok) {
                const data = await response.json();
                localStorage.setItem('ecoRutas_jwt', data.token);

                // Decodificar el rol contenido en las claims del JWT
                const payload = JSON.parse(atob(data.token.split('.')[1]));
                const rol = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role;

                // Redirección inteligente según el rol del usuario
                if (rol === 'ADMIN') {
                    window.location.href = 'dashboard.html';
                } else if (rol === 'OPERADOR') {
                    window.location.href = 'operador.html';
                } else {
                    window.location.href = 'dashboard.html'; // Fallback por defecto
                }
            }
        } catch (error) {
            // Error de red (Render dormido, sin internet, problema de CORS)
            console.error("Error de conexión:", error);
            errorDiv.textContent = 'Error de conexión con el servidor de EcoRutas.';
            errorDiv.classList.remove('hidden');
        }finally{
            // Restaurar el botón sin importar el resultado
            btnSubmit.textContent = 'Login';
            btnSubmit.disabled = false;
        }
        
    });
    } else{
        console.error("Fallo estructural: No se encontro el formulario con id 'formLogin' en el HTML")
    }
});