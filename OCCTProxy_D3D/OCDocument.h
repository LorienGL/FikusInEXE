#pragma once

#include "OCView.h"

#include <XCAFApp_Application.hxx>
#include <BinXCAFDrivers.hxx>
#include <XmlXCAFDrivers.hxx>
#include <TDocStd_Document.hxx>
#include <NCollection_Haft.h>
#include <DE_Wrapper.hxx>
#include <Message_Printer.hxx>

#include <STEPCAFControl_ConfigurationNode.hxx>
#include <IGESCAFControl_ConfigurationNode.hxx>
#include <DEXCAFCascade_ConfigurationNode.hxx>

#include <iostream>
#include <string>
#include <vector>

#include <vcclr.h>

#include "OCUtils.h"

#pragma comment(lib, "TKernel.lib")
#pragma comment(lib, "TKDE.lib")
#pragma comment(lib, "TKXCAF.lib")
#pragma comment(lib, "TKBinXCAF.lib")
#pragma comment(lib, "TKXmlXCAF.lib")
#pragma comment(lib, "TKCDF.lib")
#pragma comment(lib, "TKDECascade.lib")


public enum class OCMessageType
{
	OCMT_Info,
	OCMT_Warning,
	OCMT_Error
};

public delegate void OCMessageDelegate(OCMessageType, String^);


//DEFINE_STANDARD_HANDLE(Message_PrinterNotify, Message_Printer)
//
//class Message_PrinterNotify : public Message_Printer
//{
//	DEFINE_STANDARD_RTTIEXT(Message_PrinterNotify, Message_Printer)
//public:
//	//! Creates a printer connected to the interpretor.
//	Standard_EXPORT Message_PrinterNotify()
//	{
//	}
//
//	std::vector<std::tuple<TCollection_AsciiString, Message_Gravity> > myMessages;
//
//protected:
//	//! Send a string message with specified trace level.
//	Standard_EXPORT virtual void send(const TCollection_AsciiString& theString, const Message_Gravity theGravity) const Standard_OVERRIDE
//	{
//		const_cast<Message_PrinterNotify*>(this)->myMessages.push_back(std::tuple<TCollection_AsciiString, Message_Gravity>(theString, theGravity));
//	}
//};

public ref class OCDocument
{
protected:
	OCDocument()
	{
		//Handle(Message_PrinterNotify) aPrinter = new Message_PrinterNotify();
		//Message::DefaultMessenger()->AddPrinter(aPrinter);

		Handle(XCAFApp_Application) anApp = XCAFApp_Application::GetApplication();
		BinXCAFDrivers::DefineFormat(anApp);
		XmlXCAFDrivers::DefineFormat(anApp);
		anApp->NewDocument("BinXCAF", myDoc());
		//myDoc()->SetUndoLimit(-1);
	}

	OCDocument(System::String^ theFileName) : OCDocument()
	{
		Handle(DE_Wrapper) aSession = DE_Wrapper::GlobalWrapper();
		
		// XBF (native)
		Handle(DE_ConfigurationNode) aXBFNode = new DEXCAF_ConfigurationNode();
		aSession->Bind(aXBFNode);

		// STP
		Handle(DE_ConfigurationNode) aSTPNode = new DESTEP_ConfigurationNode();
		aSession->Bind(aSTPNode);

		// IGS
		Handle(DE_ConfigurationNode) aIGSNode = new DEIGES_ConfigurationNode();
		aSession->Bind(aIGSNode);

		// DWG

		// DXF


		if (!aSession->Read(OCUtils::StringToOCAsciiString(theFileName), myDoc()))
		{
			Handle(DE_Provider) theProvider;
			if (!aSession->FindProvider(OCUtils::StringToOCAsciiString(theFileName), Standard_True, theProvider))
				SendError("File format is not supported.\nSupported formats are: STEP, IGES, DWG, DXF and FikusIn");
			else
				SendError("File is not readable. Please verify you have rights to read the file and ensure that the file is not corrupt.");
			if (myDoc()->CanClose() == CDM_CanCloseStatus::CDM_CCS_OK)
				myDoc()->Close();
			myDoc().Nullify();
		}
	}

public:
	static OCDocument^ Create(OCMessageDelegate ^theMsgDelegate)
	{
		auto aDoc = gcnew OCDocument();
		if (aDoc->myDoc().IsNull())
			return nullptr;

		aDoc->OnMessage += theMsgDelegate;

		return aDoc;
	}

	static OCDocument^ Open(System::String^ theFileName, OCMessageDelegate^ theMsgDelegate)
	{
		auto aDoc = gcnew OCDocument(theFileName);
		if (aDoc->myDoc().IsNull())
			return nullptr;

		aDoc->OnMessage += theMsgDelegate;

		return aDoc;
	}

	static OCDocument^ Import(System::String^ theFileName, OCMessageDelegate^ theMsgDelegate)
	{
		auto aDoc = gcnew OCDocument(theFileName);
		if (aDoc->myDoc().IsNull())
			return nullptr;

		return aDoc;
	}


	bool Close()
	{
		if (myDoc().IsNull())
			return false;

		Handle(XCAFApp_Application) anApp = XCAFApp_Application::GetApplication();
		anApp->Close(myDoc());
		return true;
	}

	bool Save()
	{
		if (myDoc().IsNull())
			return false;

		Handle(XCAFApp_Application) anApp = XCAFApp_Application::GetApplication();
		if (anApp->Save(myDoc()) != PCDM_StoreStatus::PCDM_SS_OK)
			return false;

		return true;
	}

	bool SaveAs(System::String^ theFileName)
	{
		if (myDoc().IsNull())
			return false;

		Handle(XCAFApp_Application) anApp = XCAFApp_Application::GetApplication();
		anApp->SaveAs(myDoc(), OCUtils::StringToOCAsciiString(theFileName));
		return true;
	}

	bool IsEmpty()
	{
		return myDoc().IsNull();
	}


#pragma region View
public:
	bool CreateView()
	{
		if (myView != nullptr)
			return true;

		myView = OCView::Create(myDoc());
		return myView != nullptr;
	}

	OCView^ GetView()
	{
		return myView;
	}
#pragma endregion

#pragma region MessageSystem
public:
	event OCMessageDelegate^ OnMessage;
	void SendMessage(OCMessageType theType, String^ theMessage)
	{
		OnMessage(theType, theMessage);
	}
	void SendInfo(String^ theMessage)
	{
		OnMessage(OCMessageType::OCMT_Info, theMessage);
	}
	void SendWarning(String^ theMessage)
	{
		OnMessage(OCMessageType::OCMT_Warning, theMessage);
	}
	void SendError(String^ theMessage)
	{
		OnMessage(OCMessageType::OCMT_Error, theMessage);
	}

#pragma endregion


private:
	NCollection_Haft<Handle(TDocStd_Document)>             myDoc;

	OCView^ myView = nullptr;
};

