int ledRojo = 13;
int ledAmarillo = 12;
int ledVerde = 11;

int pulsador = 2;
int potenciometro = A0;

int paso = 0;
int direccion = 1;

unsigned long tiempoAnterior = 0;

int estadoAnteriorBoton = LOW;

void setup()
{
  pinMode(ledRojo, OUTPUT);
  pinMode(ledAmarillo, OUTPUT);
  pinMode(ledVerde, OUTPUT);

  pinMode(pulsador, INPUT);
}

void loop()
{
  // Leer el estado del pulsador
  int estadoBoton = digitalRead(pulsador);

  // Detectar solamente cuando se PRESIONA
  if (estadoBoton == HIGH && estadoAnteriorBoton == LOW)
  {
    direccion = direccion * -1;

    // Cambiar el paso para continuar correctamente
    // en la nueva dirección
    if (direccion == -1)
    {
      paso = 2;
    }
    else
    {
      paso = 0;
    }
  }

  // Guardar estado del botón
  estadoAnteriorBoton = estadoBoton;


  // Leer el potenciómetro
  int valor = analogRead(potenciometro);

  // Convertir el valor del potenciómetro en velocidad
  // 1000 ms = lento
  // 100 ms = rápido
  int velocidad = map(valor, 0, 1023, 1000, 100);


  // Cambiar LED según la velocidad seleccionada
  if (millis() - tiempoAnterior >= velocidad)
  {
    tiempoAnterior = millis();

    // Apagar todos los LEDs
    digitalWrite(ledRojo, LOW);
    digitalWrite(ledAmarillo, LOW);
    digitalWrite(ledVerde, LOW);


    // Dirección normal:
    // ROJO → AMARILLO → VERDE
    if (direccion == 1)
    {
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


    // Dirección inversa:
    // VERDE → AMARILLO → ROJO
    else
    {
      if (paso == 0)
        digitalWrite(ledVerde, HIGH);

      if (paso == 1)
        digitalWrite(ledAmarillo, HIGH);

      if (paso == 2)
        digitalWrite(ledRojo, HIGH);

      paso++;

      if (paso > 2)
        paso = 0;
    }
  }
}