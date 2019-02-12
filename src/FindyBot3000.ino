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
#include <ArduinoJson.h>

// Constants
#define PIXEL_PIN D3
#define PIXEL_COUNT 60*14
#define PIXEL_TYPE WS2812B
#define POWER_SUPPLY_RELAY_PIN D0

// The width, in LEDs, that a single character consumes on the LED matrix
#define LED_MATRIX_CHAR_WIDTH 6

#define ON true
#define OFF false

StaticJsonBuffer<200> jsonBuffer;

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
int textLength = 0;

bool displayOn = false;

// IFTTT Function prototypes
void googleAssistantEventHandler(const char *event, const char *data);

// Webhook response handler function prototypes
void azureFunctionEventResponseHandler(const char *event, const char *data);

// Program
void setup()
{
  Serial.begin();
  Serial.println("FindyBot3000");

  // Handle incoming Google Assistant commands, via IFTTT
  Particle.subscribe("Google_", googleAssistantEventHandler);

  // Handle Azure Function web hook response
  Particle.subscribe("hook-response/callAzureFunctionEvent", azureFunctionEventResponseHandler, MY_DEVICES);

  // Start FindyBot3000 with the distplay off
  pinMode(POWER_SUPPLY_RELAY_PIN, OUTPUT);
  digitalWrite(POWER_SUPPLY_RELAY_PIN, LOW);

  delay(1000);

  textLength = text.length();

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
  //
  // if (doTheThing)
  // {
  //   //lightBoxes();
  //   allLeds(offset++);
  //   delay(10);
  // }
  scrollDisplay();
}

struct CommandHandler
{
  const char* command;
  void (*handle) (const char* data);
};

const char* FindItem = "FindItem";
const char* FindTags = "FindTags";
const char* InsertItem = "InsertItem";
const char* RemoveItem = "RemoveItem";
const char* AddTags = "AddTags";
const char* SetBrightness = "SetBrightness";
const char* SetDisplay = "SetDisplay";

// Function callbacks
const CommandHandler commands[] =
{
  { FindItem, findItem },
  { FindTags, findTags },
  { InsertItem, insertItem },
  { RemoveItem, removeItem },
  { SetBrightness, setBrightness },
  { SetDisplay, setDisplay }
};

void googleAssistantEventHandler(const char* event, const char* data)
{
  if (event == NULL || data == NULL) return;

  Serial.printlnf("googleAssistantEventHandler event: %s, data: %s", event, data);

  // loop through each command until a match is found; then call the associated
  // handler
  for (CommandHandler cmd : commands) {
    if (strstr(event, cmd.command)) {
      cmd.handle(data);
      break;
    }
  }
}

void callAzureFunction(const char* command, const char* payload)
{
  char jsonData[255];
  sprintf(jsonData, "{\"command\":\"%s\", \"data\":\"%s\"}", command, payload);
  Serial.println(jsonData);

  // This event is tied to a webhook created in Particle Console
  // https://console.particle.io/integrations
  // The webhook calls an Azure Function, passing along with it a json payload eh
  Particle.publish("callAzureFunctionEvent", jsonData, PRIVATE);
}

// Todo - Update to fetch from DB
void findItem(const char *data)
{
  callAzureFunction(FindItem, data);
}

void findTags(const char *data)
{
  callAzureFunction(FindTags, data);
}

void insertItem(const char *data)
{
  callAzureFunction(InsertItem, data);
}

void removeItem(const char *data)
{
  callAzureFunction(RemoveItem, data);
}

// Turn the LED matrix power supply relay on or off
void setDisplay(const char *data)
{
  if (data == NULL) return;

  String onOffText = data;
  onOffText = onOffText.toLowerCase();

  if (strstr(onOffText, "on")) {
    setDisplay(true);
  } else if (strstr(onOffText, "off")) {
    setDisplay(false);
  }
}

// Set the brightness of the LED matrix, from 1 to 100, inclusive
void setBrightness(const char *data)
{
  if (data == NULL) return;

  String brightnessText = data;
  int brightness = brightnessText.toInt();

  if (0 < brightness && brightness <= 100) {
    matrix.setBrightness(map(brightness, 0, 100, 0, 255));
    matrix.show();
  }
}

struct ResponseHandler
{
  const char* command;
  void (*handle) (JsonObject& response);
};

const ResponseHandler responseHandlers[] =
{
  { FindItem, findItemResponseHandler },
  { FindTags, findTagsResponseHandler },
  { InsertItem, insertItemResponseHandler },
  { RemoveItem, removeItemResponseHandler },
};

// This function handles the webhook-response from the Azure Function
void azureFunctionEventResponseHandler(const char *event, const char *data)
{
  Serial.printlnf("azureFunctionEventResponseHandler\nevent: %s\ndata: %s", event, data);
  if (data == NULL) return;

  JsonObject& responseJson = jsonBuffer.parseObject(data);
  const char* cmd = responseJson["Command"];

  for (ResponseHandler responseHandler : responseHandlers) {
    if (strcmp(cmd, responseHandler.command) == 0) {
      responseHandler.handle(responseJson);
    }
  }

  setDisplay(ON);
}

int sRow, sCol;
uint16_t sColor;
bool sSet = false;

void findItemResponseHandler(JsonObject& json)
{
  int count = json["Count"];
  JsonObject& result = json["Result"][0];

  const char* item = result["Name"];
  int quantity = result["Quantity"];
  int row = result["Row"];
  int column = result["Column"];

  //lightBox(row, column, colors[1]);
  sRow = row;
  sCol = column;
  sColor = colors[1];
  sSet = true;

  text = item;
  textLength = text.length();

  //
  // Serial.println(data);
  //
  // JsonArray& root = jsonBuffer.parseArray(data);
  //
  // if (!root.success()) {
  //   Serial.println("parseObject() failed");
  //   return;
  // }
  //
  // Serial.println("Array size: " + root.size());
  //
  // if (root.size() > 1)
  // {
  //   return;
  // }
  //
  // JsonObject& response = root[0];
  //
  // const char* item = response["Name"];
  // int quantity = response["Quantity"];
  // int row = response["Row"];
  // int column = response["Column"];
  //
  // String responseMsg = item;
  // Serial.println("azureFunctionEventResponseHandler: " + responseMsg + ", Quantity: " + quantity + ", Row: " + row + ", Column: " + column);
  //
  // text = responseMsg;
}

void findTagsResponseHandler(JsonObject& json)
{

}

void insertItemResponseHandler(JsonObject& json)
{

}

void removeItemResponseHandler(JsonObject& json)
{

}



void lightBox(int row, int col, uint16_t color)
{
  //  if (!((0 <= row && row < 8 && 0 <= col && col < 16)
  //     || (8 <= row && row < 14 && 0 <= col && col < 8))) return;

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

  for (int i = 0; i < ledCount; i++) {
    matrix.drawPixel(ledOffset + i, row, color);
  }

  matrix.show();
}

void scrollDisplay()
{
  matrix.fillScreen(0);
  matrix.setCursor(scrollPosition, 0);
  matrix.print(text);

  for (int i = 0; i < textLength/2; i++) {
    matrix.drawBitmap(scrollPosition + i*LED_MATRIX_CHAR_WIDTH*2, 8, fire, 8, 6, colors[0 /*(scrollCount+i)%colorCount*/]);
  }

  // Change the text color on the next scroll through
  if (--scrollPosition < -textLength*LED_MATRIX_CHAR_WIDTH) {
    scrollPosition = matrix.width();
    if(++scrollCount >= colorCount) scrollCount = 0;
    matrix.setTextColor(colors[scrollCount]);
  }

  if (sSet) {
    lightBox(sRow, sCol, sColor);
  }

  matrix.show();
  //delay(10);
}

void setDisplay(bool state)
{
  if (displayOn == state) return;

  if (state) {
    digitalWrite(POWER_SUPPLY_RELAY_PIN, HIGH);
    // Give the power supply a moment to warm up if it was turned off
    // Datasheet suggests 20-50ms warm up time to support full load
    delay(2000);
  } else {
    digitalWrite(POWER_SUPPLY_RELAY_PIN, LOW);
  }

  displayOn = state;
}

/********** Testing functions **********/
uint32_t Wheel(uint8_t WheelPos)
{
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

// Light all LEDs in the matrix with a rainbow effect
void allLeds(int offset)
{
  for (int row = 0; row < 14; row++) {
    for (int col = 0; col < 60; col++) {
      matrix.drawPixel(col, row, Wheel((row*col+offset)%255));
    }
  }

  matrix.show();

  // Loop indefinitely
  // while(true) {
  //   delay(1000);
  // }
}

// Light each led-mapped box on the organizer one by one
void lightBoxes()
{
  for (int row = 0; row < 8; row++) {
    for (int col = 0; col < 16; col++) {
      lightBox(row, col, colors[r(0, colorCount)]);
      delay(50);
    }
  }
  for (int row = 8; row < 14; row++) {
    for (int col = 0; col < 8; col++) {
      lightBox(row, col, colors[r(0, colorCount)]);
      delay(50);
    }
  }
  //lightBox(r(0, 13), r(0, 16), colors[r(0, colorCount)]);
  while(true) {
    delay(1000);
  }
}

// Generate a random number between minRand and maxRand
int r(int minRand, int maxRand)
{
  return rand() % (maxRand-minRand+1) + minRand;
}
