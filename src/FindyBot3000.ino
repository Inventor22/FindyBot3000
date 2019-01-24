/*
* Project FindyBot3000
* Description: Google Assistant + IFTTT + Particle Photon + MAGIC
* Author: Dustin Dobransky
* Date: 2019-01-22
*/

// Libraries
#include <neomatrix.h>
#include <Adafruit_GFX.h>
#include <neopixel.h>

// Constants
#define PIXEL_PIN D3
#define PIXEL_COUNT 60*14
#define PIXEL_TYPE WS2812B
#define POWER_SUPPLY_RELAY_PIN D0

static const unsigned char PROGMEM fire[] = {
  0B00010000,
  0B01100100,
  0B11100010,
  0B01110000,
  0B00111000,
  0B00000000
};

const int boxLedOffsetByColumnTop[] = {
  0,
  0+4,
  0+4+3,
  0+4+3+4,
  0+4+3+4+4,
  0+4+3+4+4+3,
  0+4+3+4+4+3+4,
  0+4+3+4+4+3+4+3,
  0+4+3+4+4+3+4+3+6,
  0+4+3+4+4+3+4+3+6+4,
  0+4+3+4+4+3+4+3+6+4+3,
  0+4+3+4+4+3+4+3+6+4+3+4,
  0+4+3+4+4+3+4+3+6+4+3+4+4,
  0+4+3+4+4+3+4+3+6+4+3+4+4+3,
  0+4+3+4+4+3+4+3+6+4+3+4+4+3+4,
  0+4+3+4+4+3+4+3+6+4+3+4+4+3+4+3
};

const int boxLedWidthByColumnTop[] = {
  4, 3, 4, 3, 3, 4, 3, 4,
  4, 3, 4, 3, 3, 4, 3, 4
};

const int boxLedOffsetByColumnBottom[] = {
  0,
  0+8,
  0+8+7,
  0+8+7+7,
  0+8+7+7+9,
  0+8+7+7+9+8,
  0+8+7+7+9+8+7,
  0+8+7+7+9+8+7+7,
};

const int boxLedWidthByColumnBottom[] = {
  7, 6, 6, 7, 7, 6, 6, 7
};

// Variables
Adafruit_NeoMatrix matrix = Adafruit_NeoMatrix(
  60,        // columns
  14,        // rows
  PIXEL_PIN, // data pin
  NEO_MATRIX_TOP + NEO_MATRIX_LEFT + NEO_MATRIX_ROWS + NEO_MATRIX_ZIGZAG,
  PIXEL_TYPE);

const uint16_t colors[] =
{
  matrix.Color(255, 0, 0),   // red
  matrix.Color(0, 255, 0),   // green
  matrix.Color(0, 0, 255),   // blue
  matrix.Color(255, 0, 255), // magenta
  matrix.Color(255, 165, 0)  // orange
};

const uint8_t colorCount = sizeof(colors) / sizeof(uint16_t);

int scrollPosition = matrix.width();
int scrollCount = 0;

String text = "H I ";
int textLen = 0;

bool displayOn = true;

// Function prototypes
void findItem(const char *event, const char *data);
void setDisplay(const char *event, const char *data);
void setBrightness(const char *event, const char *data);

// Program
void setup()
{
  Serial.begin();
  Serial.println("FindyBot3000");

  Particle.subscribe("findItem", findItem);
  Particle.subscribe("setDisplay", setDisplay);
  Particle.subscribe("setBrightness", setBrightness);

  pinMode(D0, OUTPUT);
  digitalWrite(D0, HIGH);

  delay(1000);

  textLen = text.length();

  matrix.begin();
  matrix.setTextWrap(false);
  matrix.setBrightness(30);
  matrix.setTextColor(matrix.Color(255,0,255));
}

bool doTheThing = false;

int offset = 0;
void loop()
{
  if (!displayOn) return;

  if (doTheThing)
  {
    //doIt();
    allLeds(offset++);
    delay(10);
  }
  //scrollDisplay();
}

// Wheel function from https://github.com/adafruit/Adafruit_NeoPixel/blob/312693bfce447095ff0d8b6f6a1cc569415d77d7/examples/strandtest/strandtest.ino#L123
// Input a value 0 to 255 to get a color value.
// The colours are a transition r - g - b - back to r.
uint32_t Wheel(uint8_t WheelPos) {
  WheelPos = 255 - WheelPos;
  if(WheelPos < 85) {
    return matrix.Color(255 - WheelPos * 3, 0, WheelPos * 3);
  }
  if(WheelPos < 170) {
    WheelPos -= 85;
    return matrix.Color(0, WheelPos * 3, 255 - WheelPos * 3);
  }
  WheelPos -= 170;
  return matrix.Color(WheelPos * 3, 255 - WheelPos * 3, 0);
}

void allLeds(int offset)
{
  for (int row = 0; row < 14; row++)
  {
    for (int col = 0; col < 60; col++)
    {
      matrix.drawPixel(col, row, Wheel((row*col+offset)%255));
    }
  }

  matrix.show();
  //
  // while(true) {
  //   delay(1000);
  // }
}

void doIt()
{
  for (int row = 0; row < 8; row++)
  {
    for (int col = 0; col < 16; col++)
    {
      lightBox(row, col, colors[r(0, colorCount)]);
      delay(50);
    }
  }
  for (int row = 8; row < 14; row++)
  {
    for (int col = 0; col < 8; col++)
    {
      lightBox(row, col, colors[r(0, colorCount)]);
      delay(50);
    }
  }
  //lightBox(r(0, 13), r(0, 16), colors[r(0, colorCount)]);
  while(true) {
    delay(1000);
  }
}

void lightBox(int row, int col, uint16_t color)
{
   if (!((0 <= row && row < 8 && 0 <= col && col < 16)
      || (8 <= row && row < 14 && 0 <= col && col < 8))) return;

  int ledCount;
  int ledOffset;

  if (row < 8) {
    ledCount = boxLedWidthByColumnTop[col];
    ledOffset = boxLedOffsetByColumnTop[col];
  } else {
    ledCount = boxLedWidthByColumnBottom[col];
    ledOffset = boxLedOffsetByColumnBottom[col];
  }

  Serial.printlnf("row: %d, col: %d, count: %d, offset: %d", row, col, ledCount, ledOffset);

  matrix.fillScreen(0);

  for (int i = 0; i < ledCount; i++)
  {
    matrix.drawPixel(ledOffset + i, row, color);
  }

  matrix.show();
}

void scrollDisplay()
{
  matrix.fillScreen(0);
  matrix.setCursor(scrollPosition, 0);
  matrix.print(text);

  for (int i = 0; i < textLen/2; i++)
  {
    matrix.drawBitmap(scrollPosition + i*12, 8, fire, 8, 6, colors[0 /*(scrollCount+i)%colorCount*/]);
  }

  if (--scrollPosition < -textLen*6)
  {
    scrollPosition = matrix.width();
    if(++scrollCount >= colorCount) scrollCount = 0;
    matrix.setTextColor(colors[scrollCount]);
  }
  matrix.show();
  //delay(10);
}

int r(int minRand, int maxRand)
{
  return rand() % (maxRand-minRand+1) + minRand;
}

// Function callbacks
// Todo - Update to fetch from DB
void findItem(const char *event, const char *data)
{
  if (data == NULL) return;
  text = data; // built in operator to convert cStr to String
  textLen = text.length();

  doTheThing = true;
}

void setDisplay(const char *event, const char *data)
{
  if (data == NULL) return;

  String onOffText = data;
  onOffText = onOffText.toLowerCase();

  if (strstr(onOffText, "on"))
  {
    digitalWrite(POWER_SUPPLY_RELAY_PIN, HIGH);

    // Give the power supply a moment to warm up if it was turned off
    if (!displayOn)
    {
       delay(2000);
    }

    displayOn = true;
  }
  else if (strstr(onOffText, "off"))
  {
    digitalWrite(POWER_SUPPLY_RELAY_PIN, LOW);
    displayOn = false;
  }
}

void setBrightness(const char *event, const char *data)
{
  if (data == NULL) return;

  String brightnessText = data;
  int brightness = brightnessText.toInt();

  if (0 < brightness && brightness <= 100)
  {
    matrix.setBrightness(map(brightness, 0, 100, 0, 255));
    matrix.show();
  }
}
