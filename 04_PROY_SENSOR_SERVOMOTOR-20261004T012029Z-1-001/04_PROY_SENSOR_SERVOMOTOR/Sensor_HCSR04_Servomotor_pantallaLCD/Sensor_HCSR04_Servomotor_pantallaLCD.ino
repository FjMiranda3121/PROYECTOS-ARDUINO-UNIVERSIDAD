#include <Servo.h>
#include <NewPing.h>
#include <LiquidCrystal_I2C.h>

Servo servo;

// LEDs
int ledVerde = 13;
int ledAmarillo = 12;
int ledRojo = 11;

// Sensor ultrasónico
int trig = 8;
int echo = 9;

#define MAX_DISTANCE 400

NewPing sensor(trig, echo, MAX_DISTANCE);

// LCD 2004
LiquidCrystal_I2C lcd(0x27, 16, 2);


void setup() {

  servo.attach(7);

  pinMode(ledVerde, OUTPUT);
  pinMode(ledAmarillo, OUTPUT);
  pinMode(ledRojo, OUTPUT);

  // Iniciar LCD
  lcd.init();
  lcd.backlight();
}


void loop() {

  // i = ángulo

  // Barrido de ida: 0° → 180°
  for (int i = 0; i <= 180; i += 20) {

    medir(i);
  }

  // Barrido de regreso: 160° → 0°
  for (int i = 160; i >= 0; i -= 20) {

    medir(i);
  }
}


void medir(int angulo) {

  // Mover el servo
  servo.write(angulo);

  // Esperar que llegue a la posición
  delay(300);

  // Tomar distancia
  int distancia = sensor.ping_cm();


  // Apagar todos los LEDs
  digitalWrite(ledVerde, LOW);
  digitalWrite(ledAmarillo, LOW);
  digitalWrite(ledRojo, LOW);


  // LEDs según el ángulo
  if (angulo <= 20) {

    digitalWrite(ledVerde, HIGH);

  }
  else if (angulo <= 60) {

    digitalWrite(ledVerde, HIGH);
    digitalWrite(ledAmarillo, HIGH);

  }
  else {

    digitalWrite(ledVerde, HIGH);
    digitalWrite(ledAmarillo, HIGH);
    digitalWrite(ledRojo, HIGH);
  }


  // Mostrar ángulo y distancia en LCD

  lcd.clear();

  lcd.setCursor(0, 0);
  lcd.print("Angulo: ");
  lcd.print(angulo);
  lcd.print(" grados");

  lcd.setCursor(0, 1);
  lcd.print("Distancia: ");
  lcd.print(distancia);
  lcd.print(" cm");


  delay(500);
}