#include "Particle.h"
#include "neopixel.h"

// IMPORTANT: Set pixel COUNT, PIN and TYPE
#define PIXEL_PIN D3
// Only test two rows (2 meters) of LEDs
// Once the (beefy) auxiliary power supply is connected, increase the row count
// to 14 to test all LEDs.
#define PIXEL_COUNT 60*14
#define PIXEL_TYPE WS2812BD

Adafruit_NeoPixel strip(PIXEL_COUNT, PIXEL_PIN, PIXEL_TYPE);

uint32_t color = strip.Color(255, 0, 255); // magenta

void setup()
{
  strip.begin();
  strip.setBrightness(30);
  strip.show(); // Initialize all pixels to 'off'
}

void loop()
{
  pewpew(color);
}

// shoot lazers
static void pewpew(uint32_t color)
{
  static int pixels = strip.numPixels();

  for(int i = 0; i < pixels+4; i++)
  {
      strip.setPixelColor(i, color); // Color pixel
      strip.setPixelColor((i-4) , 0); // Turn off pixel
      strip.show(); // Update display
      delay(25);
  }
}
