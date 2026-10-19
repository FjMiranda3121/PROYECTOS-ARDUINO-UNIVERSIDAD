import time
import random
import requests

# 1. Configuración de la API
API_URL = "https://ecorutas-api.onrender.com/api/v2/mediciones"
API_KEY = "eco-rutas-super-secreto-2026"
CANECA_ID = "CAN-001"

def enviar_dato(distancia):
    # Se arma el contrato exacto que espera el controlador C#
    payload = {
        "canecaId": CANECA_ID,
        "distanciaCm": distancia
    }
    #Se agrega la llave
    headers = {
        "X-Api-Key": API_KEY,
        "Content-Type": "application/json"
    }

    try:
        #Disparamos el POST
        print (f" Enviando {distancia}cm...")
        response = requests.post(API_URL, json=payload, headers=headers)

        if response.status_code == 201:
            datos = response.json()

            print ("JSON recibido:", datos)

            estado = datos.get('estado', 'Desconocido')
            porcentaje = datos.get('porcentajeEstimado', 0)
            print(f"Guardado | Estado: {estado} | Llenado: {porcentaje}%")

        else:
            print(f"Error {response.status_code}: {response.text}")

    except requests.exceptions.ConnectionError:
        print("Error: No se pudo conectar a la API.")

if __name__ == "__main__":
    print("Iniciando Gateway EcoRutas (Modo Simulación)")
    print("Presionar Ctrl+C para detener el proceso. \n")

    try:
        while True:
            #Simulamos que el Arduino leyo una ditancia entre 5.0cm y 30.0cm
            distancia_simulada = round(random.uniform(10.0, 3.0), 1)

            enviar_dato(distancia_simulada)

            #Esperamos 4 segundos para no saturar la base de datos
            time.sleep(4)

    except KeyboardInterrupt:
        print("\n Gateway detenido por el usuario.")