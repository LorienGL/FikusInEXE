#pragma once

#include <d3d9.h>
#include <windows.h>

#include <Standard_Handle.hxx>
#include <NCollection_Haft.h>
#include <TDocStd_Document.hxx>

#include <WNT_Window.hxx>
#include <D3DHost_View.hxx>

#include <V3d_Viewer.hxx>
#include <V3d_View.hxx>
#include <AIS_InteractiveContext.hxx>
#include <D3DHost_GraphicDriver.hxx>

#include <WinUser.h>

#include <Quantity_Color.hxx>
#include <Quantity_ColorRGBA.hxx>


#include <V3d_DirectionalLight.hxx>
#include <V3d_AmbientLight.hxx>
#include <V3d_PositionalLight.hxx>

#include <WNT_WClass.hxx>

#include <Prs3d_LineAspect.hxx>
#include <Prs3d_DatumAspect.hxx>
#include <Prs3d_PlaneAspect.hxx>
#include <Prs3d_PointAspect.hxx>
#include <Prs3d_ShadingAspect.hxx>

#include <TPrsStd_AISViewer.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <TPrsStd_AISPresentation.hxx>
#include <XCAFPrs_Driver.hxx>

#include <AIS_Shape.hxx>


#pragma comment(lib, "TKernel.lib")
#pragma comment(lib, "TKMath.lib")
#pragma comment(lib, "TKBRep.lib")
#pragma comment(lib, "TKXSBase.lib")
#pragma comment(lib, "TKService.lib")
#pragma comment(lib, "TKV3d.lib")
#pragma comment(lib, "TKOpenGl.lib")
#pragma comment(lib, "TKD3dHost.lib")
#pragma comment(lib, "TKDEIGES.lib")
#pragma comment(lib, "TKDESTEP.lib")
#pragma comment(lib, "TKDESTL.lib")
#pragma comment(lib, "TKDEVRML.lib")
#pragma comment(lib, "TKLCAF.lib")
#pragma comment(lib, "TKVCAF.lib")


#pragma comment(lib, "D3D9.lib")

#using <system.dll>

using namespace System::Diagnostics;
using namespace System;


public ref class OCView
{
public:
    static OCView^ Create(Handle(TDocStd_Document) theDoc)
    {
        OCView^ view = gcnew OCView();
        if (view->Initialize(theDoc))
            return view;

        return nullptr;
    }

private:
	OCView() {}

    bool Initialize(Handle(TDocStd_Document) theDoc)
    {
		if (!myViewer().IsNull())
			return true;


		myGraphicDriver() = new D3DHost_GraphicDriver();
		myGraphicDriver()->ChangeOptions().buffersNoSwap = true;
		//myGraphicDriver()->ChangeOptions().contextDebug = true;

		myViewer() = new V3d_Viewer(myGraphicDriver());

		myViewer()->SetLightOff();
		Quantity_Color aBackCol = Quantity_NOC_BLACK;
		Quantity_Color::ColorFromHex("#080808", aBackCol); // Coz of a OCC bug, this is equivalent to #111111
		myViewer()->SetDefaultBackgroundColor(aBackCol);

#pragma region Lights
		gp_Pnt aPos(20, 25, -10);
		Handle(V3d_PositionalLight) aPositionalLight = new V3d_PositionalLight(aPos, Quantity_NOC_WHITE);
		aPositionalLight->SetHeadlight(Standard_True);
		aPositionalLight->SetName("headposlight");
		myViewer()->AddLight(aPositionalLight);
		myViewer()->SetLightOn(aPositionalLight);

		Handle(V3d_AmbientLight) anAmbLight = new V3d_AmbientLight(Quantity_NOC_WHITE);
		anAmbLight->SetName("amblight");
		myViewer()->AddLight(anAmbLight);
		myViewer()->SetLightOn(anAmbLight);
#pragma endregion

		myView() = myViewer()->CreateView();

		static Handle(WNT_WClass) aWClass = new WNT_WClass("OCC_Viewer", NULL, CS_OWNDC);
		Handle(WNT_Window) aWNTWindow = new WNT_Window("OCC_Viewer", aWClass, WS_POPUP, 64, 64, 64, 64);
		aWNTWindow->SetVirtual(Standard_True);
		myView()->SetWindow(aWNTWindow);

#pragma region AIS_InteractiveContext settings
		myAISContext() = new AIS_InteractiveContext(myViewer());

		myAISContext()->SetIsoNumber(0);

		myAISContext()->SetDisplayMode(AIS_Shaded, Standard_False);
		myAISContext()->DefaultDrawer()->SetDeviationAngle(1.0 * Math::PI / 180.0);
		myAISContext()->DefaultDrawer()->SetDeviationCoefficient(0.01);
		myAISContext()->DefaultDrawer()->SetMaximalChordialDeviation(0.001);

		myAISContext()->DefaultDrawer()->SetIsoOnPlane(Standard_False);
		myAISContext()->DefaultDrawer()->SetIsoOnTriangulation(Standard_False);
		myAISContext()->DefaultDrawer()->SetAutoTriangulation(Standard_True);

		myAISContext()->DefaultDrawer()->SetFaceBoundaryDraw(Standard_True);
		myAISContext()->DefaultDrawer()->SetFaceBoundaryAspect(new Prs3d_LineAspect(Quantity_NOC_GRAY10, Aspect_TOL_SOLID, 0.75));

		//myAISContext()->DefaultDrawer()->ShadingAspect()->Aspect()->SetShadingModel(Graphic3d_TypeOfShadingModel_Phong);
		myView()->SetShadingModel(V3d_PHONG);
		auto& rp = myView()->ChangeRenderingParams();
		rp.IsAntialiasingEnabled = true;
		rp.NbMsaaSamples = 4;
		rp.IsShadowEnabled = false;
		rp.IsReflectionEnabled = false;
		rp.IsTransparentShadowEnabled = false;
		rp.CollectedStats = Graphic3d_RenderingParams::PerfCounters_FrameRate;
		rp.ToShowStats = true;
		rp.TransparencyMethod = Graphic3d_RenderTransparentMethod::Graphic3d_RTM_BLEND_OIT;

		InitDefaultHilightAttributes(Prs3d_TypeOfHighlight_Dynamic, Quantity_NOC_YELLOW2, 4.0);
		InitDefaultHilightAttributes(Prs3d_TypeOfHighlight_LocalDynamic, Quantity_NOC_YELLOW2, 4.0);

		InitDefaultHilightAttributes(Prs3d_TypeOfHighlight_Selected, Quantity_NOC_ROYALBLUE2, 4.0);
		InitDefaultHilightAttributes(Prs3d_TypeOfHighlight_LocalSelected, Quantity_NOC_ROYALBLUE2, 4.0);

		myAISContext()->SetToHilightSelected(Standard_False);
#pragma endregion


		// Load document into Viewer
		if (!TPrsStd_AISViewer::Has(theDoc->Main()))
			TPrsStd_AISViewer::New(theDoc->Main(), myAISContext());

		Handle(XCAFDoc_ShapeTool) aShapeTool = XCAFDoc_DocumentTool::ShapeTool(theDoc->Main());
		TDF_LabelSequence seq;
		aShapeTool->GetFreeShapes(seq);

		for (Standard_Integer i = 1; i <= seq.Length(); i++) 
		{
			Handle(TPrsStd_AISPresentation) prs;
			if (!seq.Value(i).FindAttribute(TPrsStd_AISPresentation::GetID(), prs))
				prs = TPrsStd_AISPresentation::Set(seq.Value(i), XCAFPrs_Driver::GetID());

			prs->SetMaterial(Graphic3d_NOM_STEEL);
			prs->Display(Standard_True);
		}

		TPrsStd_AISViewer::Update(theDoc->Main());

		myAISContext()->UpdateCurrentViewer();

		myView()->FitAll();
		myView()->MustBeResized();
		myView()->Invalidate();

		StartSelection(true, true, true, true);

		return true;
	}

	void InitDefaultHilightAttributes(const Prs3d_TypeOfHighlight& theType, const Quantity_Color& theColor, const double theSize)
	{
		myAISContext()->HighlightStyle(theType)->SetMethod(Aspect_TOHM_COLOR);
		myAISContext()->HighlightStyle(theType)->SetDisplayMode(AIS_Shaded);
		myAISContext()->HighlightStyle(theType)->SetColor(theColor);

		myAISContext()->HighlightStyle(theType)->SetupOwnShadingAspect();
		myAISContext()->HighlightStyle(theType)->SetupOwnPointAspect();
		myAISContext()->HighlightStyle(theType)->SetLineAspect(new Prs3d_LineAspect(Quantity_NOC_BLACK, Aspect_TOL_SOLID, 1.0));
		myAISContext()->HighlightStyle(theType)->LineAspect()->SetAspect(myAISContext()->HighlightStyle(theType)->Link()->LineAspect()->Aspect());
		myAISContext()->HighlightStyle(theType)->SetWireAspect(new Prs3d_LineAspect(Quantity_NOC_BLACK, Aspect_TOL_SOLID, 1.0));
		myAISContext()->HighlightStyle(theType)->WireAspect()->SetAspect(myAISContext()->HighlightStyle(theType)->Link()->WireAspect()->Aspect());
		myAISContext()->HighlightStyle(theType)->SetPlaneAspect(new Prs3d_PlaneAspect());
		//*myAISContext()->HighlightStyle(theType)->PlaneAspect()->EdgesAspect() = *myAISContext()->HighlightStyle(theType)->Link()->PlaneAspect()->EdgesAspect();
		myAISContext()->HighlightStyle(theType)->SetFreeBoundaryAspect(new Prs3d_LineAspect(Quantity_NOC_BLACK, Aspect_TOL_SOLID, 1.0));
		myAISContext()->HighlightStyle(theType)->FreeBoundaryAspect()->SetAspect(myAISContext()->HighlightStyle(theType)->Link()->FreeBoundaryAspect()->Aspect());
		myAISContext()->HighlightStyle(theType)->SetUnFreeBoundaryAspect(new Prs3d_LineAspect(Quantity_NOC_BLACK, Aspect_TOL_SOLID, 1.0));
		myAISContext()->HighlightStyle(theType)->UnFreeBoundaryAspect()->SetAspect(myAISContext()->HighlightStyle(theType)->Link()->UnFreeBoundaryAspect()->Aspect());
		myAISContext()->HighlightStyle(theType)->SetDatumAspect(new Prs3d_DatumAspect());

		//myAISContext()->HighlightStyle(theType)->SetFaceBoundaryDraw(Standard_True);
		//myAISContext()->HighlightStyle(theType)->FaceBoundaryAspect()->SetColor(theColor);

		myAISContext()->HighlightStyle(theType)->ShadingAspect()->SetColor(theColor);
		myAISContext()->HighlightStyle(theType)->LineAspect()->SetColor(theColor);
		myAISContext()->HighlightStyle(theType)->PlaneAspect()->ArrowAspect()->SetColor(theColor);
		myAISContext()->HighlightStyle(theType)->PlaneAspect()->IsoAspect()->SetColor(theColor);
		myAISContext()->HighlightStyle(theType)->PlaneAspect()->EdgesAspect()->SetColor(theColor);

		myAISContext()->HighlightStyle(theType)->WireAspect()->SetColor(theColor);
		myAISContext()->HighlightStyle(theType)->FreeBoundaryAspect()->SetColor(theColor);
		myAISContext()->HighlightStyle(theType)->UnFreeBoundaryAspect()->SetColor(theColor);

		myAISContext()->HighlightStyle(theType)->PointAspect()->SetColor(theColor);
		for (Standard_Integer aPartIter = 0; aPartIter < Prs3d_DatumParts_None; ++aPartIter)
		{
			if (Handle(Prs3d_LineAspect) aLineAsp =
				myAISContext()->HighlightStyle(theType)->DatumAspect()->LineAspect((Prs3d_DatumParts)aPartIter))
			{
				aLineAsp->SetColor(theColor);
			}
		}

		myAISContext()->HighlightStyle(theType)->LineAspect()->SetWidth(theSize);

		myAISContext()->HighlightStyle(theType)->PlaneAspect()->EdgesAspect()->SetWidth(theSize);

		myAISContext()->HighlightStyle(theType)->WireAspect()->SetWidth(theSize);
		myAISContext()->HighlightStyle(theType)->FreeBoundaryAspect()->SetWidth(theSize);
		myAISContext()->HighlightStyle(theType)->UnFreeBoundaryAspect()->SetWidth(theSize);

		myAISContext()->HighlightStyle(theType)->PointAspect()->SetTypeOfMarker(Aspect_TOM_RING2);
		myAISContext()->HighlightStyle(theType)->PointAspect()->SetScale(theSize - 2.0);

		// the triangulation should be computed using main presentation attributes,
		// and should not be overridden by highlighting
		myAISContext()->HighlightStyle(theType)->SetAutoTriangulation(Standard_False);
		myAISContext()->HighlightStyle(theType)->SetZLayer(Graphic3d_ZLayerId_UNKNOWN);

		myAISContext()->HighlightStyle(theType)->SetFaceBoundaryDraw(Standard_True);
		myAISContext()->HighlightStyle(theType)->SetFaceBoundaryAspect(new Prs3d_LineAspect(theColor, Aspect_TOL_SOLID, theSize / 2.0));

		myAISContext()->HighlightStyle(theType)->EnableDrawHiddenLine();
		myAISContext()->HighlightStyle(theType)->SetHiddenLineAspect(new Prs3d_LineAspect(theColor, Aspect_TOL_DOT, theSize / 2.0));

		myAISContext()->HighlightStyle(theType)->SetTransparency(0.25);

		myAISContext()->HighlightStyle(theType)->ShadingAspect()->SetMaterial(Graphic3d_NOM_STEEL);
	}

public:

#pragma region Painting
    /// <summary> Resizes custom FBO for Direct3D output. </summary>
    System::IntPtr ResizeBridgeFBO(int theWinSizeX, int theWinSizeY)
    {
        if (myView().IsNull())
            return System::IntPtr::Zero;

        Handle(WNT_Window) aWNTWindow = Handle(WNT_Window)::DownCast(myView()->Window());
        aWNTWindow->SetPos(0, 0, theWinSizeX, theWinSizeY);
        myView()->MustBeResized();
        myView()->Invalidate();
        return System::IntPtr(Handle(D3DHost_View)::DownCast(myView()->View())->D3dColorSurface());

		Debug::WriteLine(DateTime::Now.ToString("H:mm:ss.fff") + ": ResizeBridgeFBO");
    }

    void RedrawView()
    {
        if (myView().IsNull())
            return;

		myView()->Redraw();
		//Debug::WriteLine(DateTime::Now.ToString("H:mm:ss.fff") + ":        RedrawView");
    }

	bool IsInvalidated()
	{
		return myView()->IsInvalidated();
	}

#pragma endregion

#pragma region View manipulation
	void Rotation(double theX, double theY)
	{
		//myAISContext()->ClearDetected();
		myView()->Rotation((Standard_Integer)theX, (Standard_Integer)theY);
	}

	void StartRotation(double theX, double theY, bool theZRotation)
	{
		//myAISContext()->ClearDetected();
		myView()->StartRotation((Standard_Integer)theX, (Standard_Integer)theY, theZRotation? 0.001: 0.0);
		Debug::WriteLine("StartRotation");
	}

	void Pan(double theX, double theY)
	{
		myAISContext()->ClearDetected();
		myView()->Pan((Standard_Integer)theX, (Standard_Integer)theY);
	}

	void ZoomIn()
	{
		myAISContext()->ClearDetected();
		myView()->Zoom(0, 0, 10, 10);
	}

	void ZoomOut()
	{
		myAISContext()->ClearDetected();
		myView()->Zoom(10, 10, 0, 0);
	}


#pragma region Selection
	void StartSelection(bool theFace, bool theEdge, bool theWire, bool theVertex)
	{
		myAISContext()->Deactivate();

		if (theFace)
			myAISContext()->Activate(AIS_Shape::SelectionMode(TopAbs_FACE));

		if (theEdge)
			myAISContext()->Activate(AIS_Shape::SelectionMode(TopAbs_EDGE));

		if (theWire)
			myAISContext()->Activate(AIS_Shape::SelectionMode(TopAbs_WIRE));

		if (theVertex)
			myAISContext()->Activate(AIS_Shape::SelectionMode(TopAbs_VERTEX));
	}

	void StopSelection()
	{
		myAISContext()->Deactivate();
	}

	void ClearSelection()
	{
		myAISContext()->ClearSelected(false);
	}

	void Select(int theX1, int theY1, int theX2, int theY2)
	{
		if (theX1 > theX2)
			myAISContext()->SelectionManager()->Selector()->AllowOverlapDetection(Standard_True);
		else
			myAISContext()->SelectionManager()->Selector()->AllowOverlapDetection(Standard_False);
		myAISContext()->SelectRectangle(Graphic3d_Vec2i(theX1, theY1), Graphic3d_Vec2i(theX2, theY2), myView(), AIS_SelectionScheme_Add);
		myAISContext()->UpdateCurrentViewer();
	}

	void Select(int theX, int theY)
	{
		if (!myAISContext()->HasDetected())
			myAISContext()->ClearSelected(false);
		
		myAISContext()->SelectDetected(AIS_SelectionScheme_XOR);
		myAISContext()->ClearDetected();
		myAISContext()->MoveTo(theX, theY, myView(), Standard_False);
		myAISContext()->UpdateCurrentViewer();
	}

	void MoveTo(int theX, int theY)
	{
		myAISContext()->MoveTo(theX, theY, myView(), Standard_True);
	}

	void MoveTo(int theX1, int theY1, int theX2, int theY2)
	{
		if (theX1 < theX2)
			myAISContext()->SelectionManager()->Selector()->AllowOverlapDetection(Standard_True);
		else
			myAISContext()->SelectionManager()->Selector()->AllowOverlapDetection(Standard_False);

		myAISContext()->SelectionManager()->Selector()->Pick(theX1, theY1, theX2, theY2, myView());
		myAISContext()->UpdateCurrentViewer();
	}

	void NextDetected()
	{
		myAISContext()->HilightNextDetected(myView(), Standard_True);
	}

	void PreviousDetected()
	{
		myAISContext()->HilightPreviousDetected(myView(), Standard_True);
	}


#pragma endregion

private:

    NCollection_Haft<Handle(V3d_Viewer)>             myViewer;
    NCollection_Haft<Handle(V3d_View)>               myView;
    NCollection_Haft<Handle(AIS_InteractiveContext)> myAISContext;
    NCollection_Haft<Handle(D3DHost_GraphicDriver)>  myGraphicDriver;
};

