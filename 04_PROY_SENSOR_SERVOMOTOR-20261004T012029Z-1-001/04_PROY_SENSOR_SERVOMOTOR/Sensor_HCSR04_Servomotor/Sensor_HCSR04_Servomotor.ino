#include <Servo.h>
#include <NewPing.h>

Servo servo;


int ledVerde = 13;
int ledAmarillo = 12;
int ledRojo = 11;

int trig = 6;
int echo = 5;
#define MAX_DISTANCE 400 

NewPing sensor(trig, echo, MAX_DISTANCE);

void setup() {

  servo.attach(7);

  pinMode(ledVerde, OUTPUT);
  pinMode(ledAmarillo, OUTPUT);
  pinMode(ledRojo, OUTPUT);

  Serial.begin(9600);
}

void loop(){
  //i= Es igual al angulo
  // Barrido de ida: 0° → 180°
  for(int i=0; i<=180; i +=20){

    medir(i);
  }

  // Barrido de regreso: 160° → 0°
  for(int i=160; i>=0; i -=20){

    medir(i);
  }
}

  void medir(int angulo){

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

  //Motrar angulo y distancia
  Serial.print("Angulo: ");
  Serial.print(angulo);

  Serial.print("° | Distancia: ");
  Serial.print(distancia);

  Serial.println(" cm");
  Serial.println();

  delay(500);

}
