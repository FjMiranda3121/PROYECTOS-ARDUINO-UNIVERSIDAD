
int ledRojo = 13;
int ledAmarillo = 12;
int ledVerde = 11;

void setup()
{
  pinMode(ledRojo, OUTPUT);
  pinMode(ledAmarillo, OUTPUT);
  pinMode(ledVerde, OUTPUT);
}

void loop()
{
  //LED ROJO
  digitalWrite(ledRojo, HIGH);
  digitalWrite(ledAmarillo, LOW);
  digitalWrite(ledVerde, LOW);
  delay(600); 
  
  //LED AMARILLO
  digitalWrite(ledRojo, LOW);
  digitalWrite(ledAmarillo, HIGH);
  digitalWrite(ledVerde, LOW);
  delay(500); 
  
  //LED VERDE
  digitalWrite(ledRojo, LOW);
  digitalWrite(ledAmarillo,LOW );
  digitalWrite(ledVerde, HIGH);
  delay(300); 
  
  
}