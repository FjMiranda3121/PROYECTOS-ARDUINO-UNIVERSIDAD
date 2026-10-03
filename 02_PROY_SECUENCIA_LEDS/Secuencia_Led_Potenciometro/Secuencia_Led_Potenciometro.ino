int ledRojo = 13;
int ledAmarillo = 12;
int ledVerde = 11;

int potenciometro = A0;

int paso = 0;

unsigned long tiempo = 0;

void setup()
{
  pinMode(ledRojo, OUTPUT);
  pinMode(ledAmarillo, OUTPUT);
  pinMode(ledVerde, OUTPUT);
}

void loop()
{
  int valor = analogRead(potenciometro);

  int velocidad = map(valor, 0, 1023, 1000, 100);

  if (millis() - tiempo >= velocidad)
  {
    tiempo = millis();

    digitalWrite(ledRojo, LOW);
    digitalWrite(ledAmarillo, LOW);
    digitalWrite(ledVerde, LOW);

    if (paso == 0)
      digitalWrite(ledRojo, HIGH);

    if (paso == 1)
      digitalWrite(ledAmarillo, HIGH);

    if (paso == 2)
      digitalWrite(ledVerde, HIGH);

    paso++;

    if (paso > 2)
      paso = 0;
  }
}