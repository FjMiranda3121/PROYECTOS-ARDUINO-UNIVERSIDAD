#include <WiFi.h>
#include <WiFiMulti.h>
#include <HTTPClient.h>
#include <LittleFS.h>
#include "secrets.h"

WiFiMulti wifiMulti;

// Definición de pines para el sensor HC-SR04
const int TRIG_PIN = 5;  
const int ECHO_PIN = 18; 

// Configuración del Servidor en la Nube (Render)
const char* serverUrl = "https://ecorutas-api.onrender.com/api/v3/mediciones"; 
const char* apiKey = SECRET_API_KEY; 
const char* canecaId = "CAN-001";            

// Archivo de caché local en la memoria flash
const char* CACHE_FILE = "/pendientes.txt";

// FUNCIÓN: MEDIR DISTANCIA CON FILTRO

float medirDistanciaConFiltro() {
  const int numLecturas = 5;
  float lecturas[numLecturas];

  for (int i = 0; i < numLecturas; i++) {
    digitalWrite(TRIG_PIN, LOW);
    delayMicroseconds(2);
    digitalWrite(TRIG_PIN, HIGH);
    delayMicroseconds(10);
    digitalWrite(TRIG_PIN, LOW);

    // Timeout de 30000 microsegundos (~5 metros max) para evitar bloqueos del Watchdog
    long duracion = pulseIn(ECHO_PIN, HIGH, 30000); 
    
    if (duracion == 0) {
      // Si se perdió el eco por el rebote, forzamos un valor fuera de rango para descartarlo
      lecturas[i] = 400.0; 
    } else {
      lecturas[i] = duracion * 0.0343 / 2;
    }
    
    delay(50); // Pequeña pausa acústica para que el eco anterior se disipe
  }

  // Ordenar el arreglo de lecturas de menor a mayor (Bubble Sort)
  for (int i = 0; i < numLecturas - 1; i++) {
    for (int j = i + 1; j < numLecturas; j++) {
      if (lecturas[i] > lecturas[j]) {
        float temp = lecturas[i];
        lecturas[i] = lecturas[j];
        lecturas[j] = temp;
      }
    }
  }

  // Retornar la Mediana (el valor central exacto, ignorando picos altos o bajos)
  return lecturas[numLecturas / 2];
}

// FUNCIONES DE CACHÉ Y RED (LITTLEFS)

// Función para guardar un JSON pendiente en la flash
void guardarEnCache(String payload) {
  File file = LittleFS.open(CACHE_FILE, FILE_APPEND);
  if (!file) {
    Serial.println("Error al abrir el archivo de caché para escritura.");
    return;
  }
  file.println(payload);
  file.close();
  Serial.println("[Caché Edge] Medición guardada localmente en la flash.");
}

// Función para enviar una petición HTTP POST genérica
bool enviarHttpPost(String payload) {
  if (wifiMulti.run() != WL_CONNECTED) return false;

  HTTPClient http;
  http.begin(serverUrl);
  http.addHeader("Content-Type", "application/json");
  http.addHeader("X-Api-Key", apiKey);

  int httpResponseCode = http.POST(payload);
  bool exito = (httpResponseCode > 0);

  if (exito) {
    Serial.print("API respondió con HTTP: ");
    Serial.println(httpResponseCode);
  } else {
    Serial.print("Error HTTP POST: ");
    Serial.println(httpResponseCode);
  }

  http.end();
  return exito;
}

// Función para procesar y vaciar la cola de datos pendientes en caché
void procesarCachePendiente() {
  if (!LittleFS.exists(CACHE_FILE)) return;

  File file = LittleFS.open(CACHE_FILE, FILE_READ);
  if (!file) return;

  // Si el archivo está vacío, lo cerramos y borramos
  if (file.size() == 0) {
    file.close();
    LittleFS.remove(CACHE_FILE);
    return;
  }

  Serial.println("[Caché Edge] Procesando elementos pendientes en caché...");
  
  String contenidoRestante = "";
  bool huboFallas = false;

  while (file.available()) {
    String payloadPendiente = file.readStringUntil('\n');
    payloadPendiente.trim();
    if (payloadPendiente.length() == 0) continue;

    Serial.print("Reenviando caché pendiente: ");
    Serial.println(payloadPendiente);

    if (enviarHttpPost(payloadPendiente)) {
      Serial.println("¡Elemento en caché sincronizado con éxito!");
      delay(500); // Pequeña pausa entre reenvíos
    } else {
      Serial.println("Fallo al sincronizar este elemento. Se mantendrá en caché.");
      huboFallas = true;
      contenidoRestante += payloadPendiente + "\n";
    }
  }
  file.close();

  // Reescribir el archivo únicamente con lo que no pudo enviarse (si hubo fallas)
  LittleFS.remove(CACHE_FILE);
  if (huboFallas && contenidoRestante.length() > 0) {
    File fWrite = LittleFS.open(CACHE_FILE, FILE_WRITE);
    if (fWrite) {
      fWrite.print(contenidoRestante);
      fWrite.close();
    }
  }
}
void setup() {
  Serial.begin(115200);
  delay(1000);

  pinMode(TRIG_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);

  Serial.println("\n--- ECO-RUTAS: ESP32 + HC-SR04 + LittleFS Cache ---");

  // Inicializar LittleFS
  if (!LittleFS.begin(true)) {
    Serial.println("¡Error al montar LittleFS! El sistema continuará sin caché local.");
  } else {
    Serial.println("Sistema de archivos LittleFS montado correctamente.");
  }

  // Configuración de redes multi-red (WiFiMulti)
  wifiMulti.addAP(SECRET_WIFI_CASA, SECRET_WIFI_PASS_CASA);
  wifiMulti.addAP(SECRET_WIFI_FRANK, SECRET_WIFI_PASS_FRANK);
  wifiMulti.addAP(SECRET_WIFI_JUAN, SECRET_WIFI_PASS_JUAN);


  Serial.println("Buscando redes Wi-Fi disponibles...");

  while (wifiMulti.run() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("\n¡Conexión Wi-Fi exitosa!");
  Serial.print("IP asignada: ");
  Serial.println(WiFi.localIP());
}



void loop() {
  // 1. Tomar lectura física con el HC-SR04
  digitalWrite(TRIG_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIG_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIG_PIN, LOW);

  long duracion = pulseIn(ECHO_PIN, HIGH);
  float distanciaCm = duracion * 0.0343 / 2;

  if (distanciaCm <= 0 || distanciaCm > 400) {
    Serial.println("Lectura fuera de rango del sensor. Omitiendo envío.");
    delay(5000);
    return;
  }

  Serial.print("Distancia medida: ");
  Serial.print(distanciaCm);
  Serial.println(" cm");

  // Construir el payload JSON actual
  String payloadActual = "{\"canecaId\":\"" + String(canecaId) + "\",\"distanciaCm\":" + String(distanciaCm) + "}";

  // 2. Intentar vaciar primero la caché pendiente si hay conexión
  if (wifiMulti.run() == WL_CONNECTED) {
    procesarCachePendiente();

    // 3. Enviar la medición actual
    Serial.println("Enviando telemetría actual a Render...");
    if (!enviarHttpPost(payloadActual)) {
      Serial.println("Servidor inalcanzable. Guardando medición actual en caché flash.");
      guardarEnCache(payloadActual);
    }
  } else {
    Serial.println("Wi-Fi desconectado. Guardando telemetría en caché local.");
    guardarEnCache(payloadActual);
  }

  // Esperar 10 segundos antes de la siguiente lectura
  delay(10000);
}