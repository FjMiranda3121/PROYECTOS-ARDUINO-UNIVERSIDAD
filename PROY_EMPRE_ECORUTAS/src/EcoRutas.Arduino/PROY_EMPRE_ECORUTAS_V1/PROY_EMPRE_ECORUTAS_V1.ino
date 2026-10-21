#include <Wire.h>
#include <LiquidCrystal_I2C.h>

LiquidCrystal_I2C lcd(0x27, 16, 2);

const int trigPin = 9;
const int echoPin = 10;

// Estas medidas debemos ajustarlas a nuestra caneca
const float distanciaVacio = 30.0;
const float distanciaLleno = 6.0;

void setup() {
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);

  Serial.begin(9600);

  lcd.init();
  lcd.backlight();

  lcd.setCursor(0, 0);
  lcd.print("Contenedor");
  lcd.setCursor(0, 1);
  lcd.print("Iniciando...");

  delay(2000);
  lcd.clear();
}

void loop() {

  // Enviar pulso al HC-SR04
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);

  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);

  digitalWrite(trigPin, LOW);

  // Recibir tiempo del eco
  long duracion = pulseIn(echoPin, HIGH);

  // Convertir tiempo a distancia en centimetros
  float distancia = duracion * 0.0343 / 2;

  // Convertir distancia en porcentaje de llenado
  float nivel = ((distanciaVacio - distancia) /
                 (distanciaVacio - distanciaLleno)) * 100;

  // Limitar el porcentaje entre 0 y 100
  if (nivel < 0) {
    nivel = 0;
  }

  if (nivel > 100) {
    nivel = 100;
  }

  // Determinar estado
  String estado;

  if (nivel < 60) {
    estado = "NORMAL";
  }
  else if (nivel < 80) {
    estado = "ATENCION";
  }
  else {
    estado = "CRITICO";
  }

  // Mostrar en Monitor Serial
  Serial.print("Distancia: ");
  Serial.print(distancia);
  Serial.print(" cm | Nivel: ");
  Serial.print(nivel);
  Serial.print("% | Estado: ");
  Serial.println(estado);

  // Mostrar en LCD
  lcd.clear();

  lcd.setCursor(0, 0);
  lcd.print("Nivel: ");
  lcd.print((int)nivel);
  lcd.print("%");

  lcd.setCursor(0, 1);
  lcd.print("Estado: ");
  lcd.print(estado);

  delay(1000);
}