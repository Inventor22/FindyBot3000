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

#define LED_ROWS 8+6
#define LED_COLS 60
#define LED_COLS_HALF (LED_COLS/2)

// The width, in LEDs, that a single character consumes on the LED matrix
#define LED_MATRIX_CHAR_WIDTH 6

#define ON true
#define OFF false

StaticJsonBuffer<3000> jsonBuffer;

const PROGMEM unsigned char fire[] = {
  0B00010000,
  0B01100100,
  0B11100010,
  0B01110000,
  0B00111000,
  0B00000000
};

const PROGMEM unsigned char smile[] = {
  0B00000000, 0B00000000,
  0B00011100, 0B00111000,
  0B00100010, 0B01000100,
  0B00000000, 0B00000000,
  0B11000000, 0B00000011,
  0B01100000, 0B00000110,
  0B00011111, 0B11111000,
  0B00000000, 0B00000000
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

uint16_t red = matrix.Color(255, 0, 0);
uint16_t green = matrix.Color(0, 255, 0);
uint16_t blue = matrix.Color(0, 0, 255);
uint16_t magenta = matrix.Color(255, 0, 255);
uint16_t orange = matrix.Color(255, 165, 0);
uint16_t cyan = matrix.Color(0, 255, 255);

const uint16_t colors[] = {
  red,
  green,
  blue,
  magenta,
  orange,
  cyan
};

const uint8_t colorCount = sizeof(colors) / sizeof(uint16_t);

int scrollPosition = matrix.width();
int scrollCount = 0;

String text = "H I ";
int textLength = 0;

// Controlled via Google Assistant
bool enableDisplay = false;
bool enableTextScrolling = false;
bool enableDebugging = true;

// Controlled locally
bool enableLightAllBoxes = false;
bool enableRainbowLeds = false;

// Google Assistant > IFTTT > Particle Photon subscribe event handler function prototype
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

  // Start FindyBot3000 with the display off
  pinMode(POWER_SUPPLY_RELAY_PIN, OUTPUT);
  digitalWrite(POWER_SUPPLY_RELAY_PIN, OFF);
  delay(1000);

  textLength = text.length();

  matrix.begin();
  matrix.setTextWrap(false);
  matrix.setBrightness(30);
  matrix.setTextColor(matrix.Color(255,0,255));

  setDisplay(ON);

  greenRedGradientTest();
}

void loop()
{
  if (!enableDisplay)      return;
  if (enableLightAllBoxes) lightBoxes();
  if (enableTextScrolling) scrollDisplay();
  if (enableRainbowLeds)   rainbowLeds();
}

// weight = 0 -> col0, weight = 0.5 -> 50/50 col0/col1, weight = 1 -> col1
uint16_t getGradientColor(uint16_t col0, uint16_t col1, float value)
{
  uint8_t red = 0, green = 0, blue = 0;
  uint8_t r = (col0 & 0xF800) >> 8;
  uint8_t g = (col0 & 0x07E0) >> 3;
  uint8_t b = (col0 & 0x1F) << 3;
  // r = (r * 255) / 31;
  // g = (g * 255) / 63;
  // b = (b * 255) / 31;

  if (r > 0) red = r;
  if (g > 0) green = g;
  if (b > 0) blue = b;

  r = (col1 & 0xF800) >> 8;
  g = (col1 & 0x07E0) >> 3;
  b = (col1 & 0x1F) << 3;

  if (r > 0) red = r;
  if (g > 0) green = g;
  if (b > 0) blue = b;

  if (red & blue) {
    red = value <= 0.5 ? 255 : (255 - 255*(value-0.5)*2);
    blue = value <= 0.5 ? 255 * (value*2) : 255;
  }
  else if (red & green) {
    red = value <= 0.5 ? 255 : (255 - 255*(value-0.5)*2);
    green = value <= 0.5 ? 255 * (value*2) : 255;
  } else { // green & blue
    green = value <= 0.5 ? 255 : (255 - 255*(value-0.5)*2);
    blue = value <= 0.5 ? 255 * (value*2) : 255;
  }
  return matrix.Color(red, green, blue);
}

// 0.0 = Red, 0.5 = Yellow, 1.0 = Green
uint16_t getGreenRedValue(float value)
{
  int red = value <= 0.5 ? 255 : (255 - 255*(value-0.5)*2);
  int green = value <= 0.5 ? 255 * (value*2) : 255;
  return matrix.Color(red, green, 0);
}

void greenRedGradientTest()
{
  int row = 0;

  matrix.fillScreen(0);

  for (int i = 0; i < LED_COLS; i++)
  {
    int red = min(255 * (((float)i)/LED_COLS_HALF), 255);
    int green = (i < LED_COLS_HALF) ? 255 : (255 - 255*(((float)(i - LED_COLS_HALF))/LED_COLS_HALF));
    matrix.drawPixel(i, row, matrix.Color(red, 0, 0));
    matrix.drawPixel(i, row+1, matrix.Color(0, green, 0));
    matrix.drawPixel(i, row+2, matrix.Color(red, green, 0));
  }

  for (int i = 0; i < LED_COLS; i++)
  {
    matrix.drawPixel(i, row+3, getGreenRedValue(((float)i)/LED_COLS));
  }

  for (int i = 0; i < LED_COLS; i++)
  {
    matrix.drawPixel(i, row+5, getGradientColor(green, blue, ((float)i)/LED_COLS));
    matrix.drawPixel(i, row+6, getGradientColor(blue, red, ((float)i)/LED_COLS));
    matrix.drawPixel(i, row+7, getGradientColor(red, green, ((float)i)/LED_COLS));
  }

  matrix.show();

  delay(1000);
}

struct CommandHandler
{
  const char* command;
  void (*handle) (const char* data);
};

// Requires AzureFunction
const char* FindItem = "FindItem";
const char* FindTags = "FindTags";
const char* InsertItem = "InsertItem";
const char* RemoveItem = "RemoveItem";
const char* AddTags = "AddTags";
const char* SetQuantity = "SetQuantity";
const char* UpdateQuantity = "UpdateQuantity";
const char* ShowAllBoxes = "ShowAllBoxes";
const char* BundleWith = "BundleWith";
const char* HowMany = "HowMany";
const char* UnknownCommand = "UnknownCommand";

// Processed on Particle Photon
const char* SetBrightness = "SetBrightness";
const char* SetDisplay = "SetDisplay";
const char* SetDebugging = "SetDebugging";
const char* SetScrollText = "SetScrollText";


// Function callbacks
const CommandHandler commands[] =
{
  { FindItem, findItem },
  { FindTags, findTags },
  { InsertItem, insertItem },
  { RemoveItem, removeItem },
  { AddTags, addTags },
  { SetQuantity, setQuantity},
  { UpdateQuantity, updateQuantity},
  { SetBrightness, setBrightness },
  { SetDisplay, setDisplay },
  { SetDebugging, setDebugging },
  { SetScrollText, setScrollText },
  { ShowAllBoxes, showAllBoxes },
  { BundleWith, bundleWith },
  { HowMany, howMany }
};

void googleAssistantEventHandler(const char* event, const char* data)
{
  if (event == NULL || data == NULL) return;

  Serial.printlnf("googleAssistantEventHandler event: %s, data: %s", event, data);

  // loop through each command until a match is found; then call the associated handler
  for (CommandHandler cmd : commands) {
    if (strstr(event, cmd.command)) {
      cmd.handle(data);
      break;
    }
  }
}

void callAzureFunction(const char* command, const char* payload, bool isJson = false)
{
  char jsonData[255];
  if (isJson) {
    sprintf(jsonData, "{\"command\":\"%s\", \"data\":%s}", command, payload);
  } else {
    sprintf(jsonData, "{\"command\":\"%s\", \"data\":\"%s\"}", command, payload);
  }
  Serial.println(jsonData);

  // This event is tied to a webhook created in Particle Console
  // https://console.particle.io/integrations
  // The webhook calls an Azure Function, passing along with it a json payload eh
  Particle.publish("callAzureFunctionEvent", jsonData, PRIVATE);
}

/* ============= GOOGLE ASSISTANT EVENT HANDLERS ============= */

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
  callAzureFunction(InsertItem, data, true);
}

void removeItem(const char *data)
{
  callAzureFunction(RemoveItem, data);
}

void addTags(const char *data)
{
  callAzureFunction(AddTags, data);
}

void setQuantity(const char *data)
{
  callAzureFunction(SetQuantity, data, true);
}

void updateQuantity(const char *data)
{
  callAzureFunction(UpdateQuantity, data, true);
}

void showAllBoxes(const char *data)
{
  callAzureFunction(ShowAllBoxes, data);
}

void bundleWith(const char *data)
{
  callAzureFunction(BundleWith, data, true);
}

void howMany(const char *data)
{
  callAzureFunction(HowMany, data);
}

// Turn the LED matrix power supply relay on or off
void setDisplay(const char *data)
{
  if (data == NULL) return;

  if (strstr(data, "on")) {
    setDisplay(true);
  } else if (strstr(data, "off")) {
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

void setDebugging(const char *data)
{
  setStateFromText(enableDebugging, data);
}

void setScrollText(const char *data)
{
  setStateFromText(enableTextScrolling, data);
}

void setStateFromText(bool& variable, const char *onOffText)
{
  if (strcmp(onOffText, "on") == 0) {
    variable = true;
  }
  else if (strcmp(onOffText, "off") == 0) {
    variable = false;
  }
}

/* ============= WEBHOOK RESPONSE HANDLERS ============= */

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
  { AddTags, addTagsResponseHandler },
  { SetQuantity, setQuantityResponseHandler },
  { UpdateQuantity, updateQuantityResponseHandler },
  { ShowAllBoxes, showAllBoxesResponseHandler },
  { BundleWith, bundleWithResponseHandler },
  { HowMany, howManyResponseHandler },
  { UnknownCommand, unknownCommandResponseHandler }
};

char msg[600];
// This function handles the webhook-response from the Azure Function
void azureFunctionEventResponseHandler(const char *event, const char *data)
{
  Serial.printlnf("azureFunctionEventResponseHandler\nevent: %s\ndata: %s", event, data);
  if (data == NULL) return;

  // remove all backslashes ('\') added by particle webhook-response
  int dataLen = strlen(data);
  int j = 0;
  for (int i = 1; i < dataLen-1; i++)
  {
    if (data[i] == '\\') continue;
    msg[j++] = data[i];
  }
  msg[j] = '\0'; // Terminate the string

  //strcpy(msg, data);
  Serial.println("------------------------------------");
  Serial.println(msg);
  Serial.println(strlen(msg));
  Serial.println("------------------------------------");

  jsonBuffer.clear(); // Aha! This is what I needed to fix multiple FindItem calls.
  JsonObject& responseJson = jsonBuffer.parseObject(msg);

  if (!responseJson.success()) {
    Serial.println("Parsing JSON failed");
    return;
  }

  const char* cmd = responseJson["Command"];

  Serial.print("Command: ");
  Serial.println(cmd);

  for (ResponseHandler responseHandler : responseHandlers) {
    if (strcmp(cmd, responseHandler.command) == 0) {
      responseHandler.handle(responseJson);
      break;
    }
  }
}

int sRow, sCol;
uint16_t sColor;
bool sSet = false;

void findItemResponseHandler(JsonObject& json)
{
  int count = json["Count"];
  if (count <= 0) {
    Serial.println("Item not found");
    dispayItemNotFound();
  } else {
    JsonObject& result = json["Result"][0];

    const char* item = result["Name"];
    int quantity = result["Quantity"];
    int row = result["Row"];
    int col = result["Col"];

    //lightBox(row, column, green);
    sRow = row;
    sCol = col;
    sColor = green;
    sSet = true;

    setDisplay(ON);
    matrix.fillScreen(0);
    lightBox(row, col, green);
    matrix.show();

    Serial.printlnf("item: %s, row: %d, col: %d, quantity: %d", item, row, col, quantity);

    text = item;
    textLength = text.length();
  }
}

void findTagsResponseHandler(JsonObject& json)
{
  const char* cmd = json["Command"];
  int count = json["Count"];
  int numTags = json["Tags"];

  if (count <= 0) {
    Serial.println("FindTags returned 0 items");
    return;
  }

  JsonArray& items = json["Result"];

  setDisplay(ON);
  matrix.fillScreen(0);

  for (int i = 0; i < count; i++)
  {
     //const char* name = items[i]["Name"];
     JsonArray& info = items[i];
     int row = info[0];
     int col = info[1];
     float confidence = ((float)info[2])/numTags;

     //Serial.printlnf("Name: %s, Row: %d, Col: %d, Confidence: %f", name, row, col, confidence);
     Serial.printlnf("Row: %d, Col: %d, Confidence: %f", row, col, confidence);

     lightBox(row, col, getGreenRedValue(confidence));
  }

  matrix.show();
}

void insertItemResponseHandler(JsonObject& json)
{
  bool success = json["Success"];

  if (!success) {
    Serial.println("InsertItem failed");
    return;
  }

  int row = json["Row"];
  int col = json["Col"];

  matrix.fillScreen(0);
  lightBox(row, col, green);
  matrix.show();

  Serial.printlnf("row: %d, col: %d", row, col);
}

void removeItemResponseHandler(JsonObject& json)
{
  Serial.println("removeItemResponseHandler");

  if (!json["Success"]) {
    Serial.println("RemoveItem failed");
    return;
  }
}

void addTagsResponseHandler(JsonObject& json)
{
  Serial.println("addTagsResponseHandler");

  if (!json["Success"]) {
    Serial.println("AddTags failed");
    return;
  }
}

// Modifying quantity triggers FindItem response handler
void setQuantityResponseHandler(JsonObject& json)
{
  Serial.println("setQuantityResponseHandler");

  if (!json["Success"]) {
    Serial.println("SetQuantity failed");
    return;
  }

  JsonObject& result = json["Result"][0];
  int row = result["Row"];
  int col = result["Col"];

  matrix.fillScreen(0);
  lightBox(row, col, green);
  matrix.show();

  Serial.printlnf("row: %d, col: %d", row, col);
}

void updateQuantityResponseHandler(JsonObject& json)
{
  Serial.println("updateQuantityResponseHandler");

  if (!json["Success"]) {
    Serial.println("UpdateQuantity failed");
    return;
  }

  JsonObject& result = json["Result"][0];
  int row = result["Row"];
  int col = result["Col"];

  matrix.fillScreen(0);
  lightBox(row, col, green);
  matrix.show();

  Serial.printlnf("row: %d, col: %d", row, col);
}

void showAllBoxesResponseHandler(JsonObject& json)
{
  Serial.println("showAllBoxesResponseHandler");

  int count = json["Count"];

  if (count == 0)
  {
    Serial.println("ShowAllBoxes returned 0 entries");
    return;
  }

  const char* coordsJson = json["Coords"];
  matrix.fillScreen(0);

  for (int i = 0; i < count*2; i += 2)
  {
    int row = coordsJson[i] - 'a';
    int col = coordsJson[i+1] - 'a';
    //Serial.printf("[%d,%d],", row, col);
    lightBox(row, col, colors[r(0, colorCount-1)]);
  }
  //Serial.println();

  matrix.show();
}

void bundleWithResponseHandler(JsonObject& json)
{
  Serial.println("bundleWithResponseHandler");

  if (!json["Success"]) {
    Serial.println("UpdateQuantity failed");
    return;
  }

  const char* newItem = json["NewItem"];
  int row = json["Row"];
  int col = json["Col"];
  int quantity = json["Quantity"];

  const char* existingItem = json["ExistingItem"];

  matrix.fillScreen(0);
  lightBox(row, col, green);
  matrix.show();

  Serial.printlnf("NewItem: %s, row: %d, col: %d, quantity: %d, ExistingItem: %s", newItem, row, col, quantity, existingItem);
}

void howManyResponseHandler(JsonObject& json)
{
  Serial.println("howManyResponseHandler");

  if (!json["Success"]) {
    Serial.println("HowMany failed");
    return;
  }

  int quantity = json["Quantity"];
  int row = json["Row"];
  int col = json["Col"];

  matrix.fillScreen(0);
  matrix.setCursor(3, 0);
  matrix.print(quantity);
  lightBox(row, col, green);
  matrix.show();
}

void unknownCommandResponseHandler(JsonObject& json)
{
  Serial.println("unknownCommandResponseHandler");
  const char* unknownCmd = json["Command"];
  Serial.println(unknownCmd);
}

/* =============== HELPER FUNCTIONS =============== */

void dispayItemNotFound()
{
  setDisplay(ON);
  matrix.fillScreen(0);
  matrix.drawPixel(29, 7, red);
  matrix.drawPixel(30, 7, red);
  matrix.show();
}

void lightBox(int row, int col, uint16_t color)
{
  //  if (!((0 <= row && row < 8 && 0 <= col && col < 16)
  //     || (8 <= row && row < 14 && 0 <= col && col < 8))) return;

  int ledCount;
  int ledOffset;

  if (row < 8 && col < 16) {
    ledCount = boxLedWidthByColumnTop[col];
    ledOffset = boxLedOffsetByColumnTop[col];
  } else if (row < 16 && col < 8) {
    ledCount = boxLedWidthByColumnBottom[col];
    ledOffset = boxLedOffsetByColumnBottom[col];
  } else {
    Serial.printlnf("Invalid. Row: %d, Col: %d\n", row, col);
  }

  //Serial.printlnf("row: %d, col: %d, count: %d, offset: %d", row, col, ledCount, ledOffset);

  //matrix.fillScreen(0);

  for (int i = 0; i < ledCount; i++) {
    matrix.drawPixel(ledOffset + i, row, color);
  }

  //matrix.show();
}

void scrollDisplay()
{
  static const int smileOffset = 16+8;

  matrix.fillScreen(0);
  matrix.setCursor(scrollPosition, 0);
  matrix.print(text);

  for (int i = 0; i < textLength/2 + (smileOffset/LED_MATRIX_CHAR_WIDTH)-2; i++) {
    matrix.drawBitmap(scrollPosition + i*LED_MATRIX_CHAR_WIDTH*2, 8, fire, 8, 6, colors[0 /*(scrollCount+i)%colorCount*/]);
  }

  matrix.drawBitmap(scrollPosition + textLength*LED_MATRIX_CHAR_WIDTH + 8, 0, smile, 16, 8, colors[2]);

  // Change the text color on the next scroll through
  if (--scrollPosition < -textLength*LED_MATRIX_CHAR_WIDTH - smileOffset) {
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
  if (enableDisplay == state) return;

  if (state) {
    digitalWrite(POWER_SUPPLY_RELAY_PIN, ON);
    // Give the power supply a moment to warm up if it was turned off
    // Datasheet suggests 20-50ms warm up time to support full load
    delay(1000);
  } else {
    digitalWrite(POWER_SUPPLY_RELAY_PIN, OFF);
  }

  enableDisplay = state;
}

/********** TESTING FUNCTIONS **********/

// Light all LEDs in the matrix with a rainbow effect
void rainbowLeds()
{
  static int offset = 0;
  for (int row = 0; row < 14; row++) {
    for (int col = 0; col < 60; col++) {
      matrix.drawPixel(col, row, Wheel((row*col+offset)%255));
    }
  }
  offset++;
  matrix.show();
  delay(1000);
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
  while(true) {
    delay(1000);
  }
}

// Borrowed from: https://learn.adafruit.com/multi-tasking-the-arduino-part-3/utility-functions
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

// Generate a random number between minRand and maxRand
int r(int minRand, int maxRand)
{
  return rand() % (maxRand-minRand+1) + minRand;
}
