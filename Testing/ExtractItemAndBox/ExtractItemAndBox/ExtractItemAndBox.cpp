// ExtractItemAndBox.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include <iostream>

int main()
{
    std::cout << "Hello World!\n"; 

	int quantity = 22;
	const char* data = "Purple LED into a small box";
	const char* itemAndBox = data;
	bool isSmallBox = strstr(itemAndBox, "into a small box") > 0;

	const char* itemEnd = strstr(data, " into a ");
	const int itemLength = itemEnd - data;

	char* item = new char[itemLength + 1];
	memcpy(item, itemAndBox, itemLength);

	item[itemLength] = '\0';
	std::cout << item << std::endl;

	char jsonData[100];
	sprintf_s(jsonData, "{\"Item\":\"%s\",\"Quantity\":%d,\"IsSmallBox\":%s}", item, quantity, (isSmallBox ? "true" : "false"));
	
	std::cout << jsonData << std::endl;
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
