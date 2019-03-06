
 #include <Adafruit_mfGFX.h>
 #include "Adafruit_ILI9341.h"

 #define LCD_DC D0
 #define LCD_CS A2
 #define LCD_RST D2

Adafruit_ILI9341 lcd = Adafruit_ILI9341(LCD_CS, LCD_DC, LCD_RST);

void setup() {
  Serial.begin(9600);
  Serial.println("ILI9341 Testing");

  lcd.begin();

  lcd.fillScreen(ILI9341_GREEN);
  yield();
  delay(1000);

  lcd.setCursor(0, 0);
  lcd.setTextColor(ILI9341_BLUE);
  lcd.setTextSize(1);
  lcd.println("Hello World!");

  lcd.setTextColor(ILI9341_CYAN);
  lcd.setTextSize(2);
  lcd.println("FindyBot3000");
  delay(2000);

  int width = lcd.width();
  int height = lcd.height();
  lcd.drawLine(0, 0, width, height, ILI9341_GREEN);

  lcd.fillScreen(ILI9341_BLACK);
  for(int y=0; y < height; y++) {
    lcd.drawFastHLine(0, y, 1, ILI9341_RED);
  }

  lcd.drawRect(50, 50, 150, 150, ILI9341_BLUE);
  lcd.fillRect(55, 55, 90, 90, ILI9341_MAGENTA);
  lcd.fillCircle(width/2, height/2, 40, ILI9341_GREEN);
  lcd.drawCircle(width/2, height/2, 45, ILI9341_GREEN);
}


void loop(void) {
  delay(1000);
}
