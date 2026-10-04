#include <Servo.h>

Servo servo;

int potenciometro = A0;

int ledVerde = 13;
int ledAmarillo = 12;
int ledRojo = 11;

void setup() {

  servo.attach(9);

  pinMode(ledVerde, OUTPUT);
  pinMode(ledAmarillo, OUTPUT);
  pinMode(ledRojo, OUTPUT);

  Serial.begin(9600);
}

void loop() {

  int lectura = analogRead(potenciometro);

  int angulo = map(lectura, 0, 1023, 0, 180);

  servo.write(angulo);

  digitalWrite(ledVerde, LOW);
  digitalWrite(ledAmarillo, LOW);
  digitalWrite(ledRojo, LOW);

  if (angulo < 60) {

    digitalWrite(ledVerde, HIGH);

  } 
  else if (angulo < 120) {

    digitalWrite(ledVerde, HIGH);
    digitalWrite(ledAmarillo, HIGH);

  } 
  else {

    digitalWrite(ledVerde, HIGH);
    digitalWrite(ledAmarillo, HIGH);
    digitalWrite(ledRojo, HIGH);
  }

  Serial.print("Potenciometro: ");
  Serial.print(lectura);

  Serial.print(" | Angulo: ");
  Serial.println(angulo);

  delay(100);
}