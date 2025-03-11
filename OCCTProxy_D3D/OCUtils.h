#pragma once


#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <vcclr.h>


#pragma comment(lib, "TKernel.lib")


using namespace System;

ref class OCUtils
{
public:
	static TCollection_AsciiString StringToOCAsciiString(System::String^ theString)
	{
		if (theString == nullptr)
		{
			return TCollection_AsciiString();
		}
		pin_ptr<const wchar_t> aPinChars = PtrToStringChars(theString);
		const wchar_t* aWCharPtr = aPinChars;
		if (aWCharPtr == NULL
			|| *aWCharPtr == L'\0')
		{
			return TCollection_AsciiString();
		}
		return TCollection_AsciiString(aWCharPtr);
	}

	static TCollection_ExtendedString StringToOCExtendedString(System::String^ theString)
	{
		if (theString == nullptr)
		{
			return TCollection_ExtendedString();
		}
		pin_ptr<const wchar_t> aPinChars = PtrToStringChars(theString);
		const wchar_t* aWCharPtr = aPinChars;
		if (aWCharPtr == NULL
			|| *aWCharPtr == L'\0')
		{
			return TCollection_ExtendedString();
		}
		return TCollection_ExtendedString(aWCharPtr);
	}
};



