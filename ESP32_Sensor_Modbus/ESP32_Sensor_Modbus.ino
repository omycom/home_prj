#include <WiFi.h>
#include <ModbusIP_ESP8266.h>

ModbusIP mb;

const int humidityPin = 34;
const int volumePin = 35;

const int echo  = 26;
const int trig = 25;

const int dry = 4095;
const int wet = 3500;

int rhreg_1 = 0;
int rhreg_2 = 1;
int rhreg_3 = 2;

int led_builtin = 2;

void setup() {
  Serial.begin(9600);

  pinMode(2, OUTPUT);

  WiFi.begin("your_Wifi", "your_password");
  while (WiFi.status() != WL_CONNECTED) {
      delay(500);
      Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected.");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());

  pinMode(trig, OUTPUT);
  pinMode(echo, INPUT);

  mb.server();

  mb.addHreg(rhreg_1);
  mb.addHreg(rhreg_2);
  mb.addHreg(rhreg_3);

  delay(1000);
}

void loop() {

  digitalWrite(led_builtin, HIGH);

  long distance = GetDistance();
  int iDistance = (int)distance;

  int humidityPersent = map(analogRead(humidityPin), wet, dry, 100, 0);
  int volumePersent = map(analogRead(volumePin), 0, 4095, 100, 0);

  Serial.print(humidityPersent);
  Serial.println("%-humidity");

  Serial.print(volumePersent);
  Serial.println("%-volume");

  Serial.print(iDistance);
  Serial.println("cm-distance");

  mb.Hreg(rhreg_1, humidityPersent);
  mb.Hreg(rhreg_2, volumePersent);
  mb.Hreg(rhreg_3, iDistance);
  mb.task();

  digitalWrite(led_builtin, LOW);
  delay(200);
}

long GetDistance() {
  long duration;

  digitalWrite(trig, LOW);
  delayMicroseconds(2);
  digitalWrite(trig, HIGH);
  delayMicroseconds(10);
  digitalWrite(trig, LOW);

  duration = pulseIn (echo, HIGH);
  return duration * 17 / 1000;
} 