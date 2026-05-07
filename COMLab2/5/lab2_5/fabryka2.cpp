#include "fabryka2.h"
#include "IKlasa2.h"
#include <new>
#include "Klasa2.h"


extern volatile ULONG numberOfUsedInstances;


fabryka2::fabryka2()
{
	 /*
	Tutaj dodaj inkrementację liczby istniejących instancji oraz 
        inicjalizację wartości zmiennej do licznika referencji.
	 */
	InterlockedIncrement(&numberOfUsedInstances);
	m_ref = 0;
}


fabryka2::~fabryka2()
{
	 /*
	Tutaj dodaj dekrementację liczby istniejących instancji oraz 
	 */
	InterlockedDecrement(&numberOfUsedInstances);
}

HRESULT fabryka2::QueryInterface(REFIID InterfaceIdentifier, void ** InterfacePointer)
{
	if (InterfacePointer == NULL) return E_POINTER;
	*InterfacePointer = NULL;
	if (InterfaceIdentifier == IID_IUnknown || InterfaceIdentifier == IID_IClassFactory) *InterfacePointer = this;
	if (*InterfacePointer != NULL) {
		AddRef();
		return S_OK;
	};
	return E_NOINTERFACE;
}

ULONG fabryka2::AddRef()
{
	/*
	Tutaj zaimplementuj dodawanie referencji na obiekt.
	 */
	InterlockedIncrement(&m_ref);
	return m_ref;
}

ULONG fabryka2::Release()
{
	/*
	Tutaj zaimplementuj usuwania referencji na obiekt.
	Jeżeli nie istnieje żadna referencja obiekt powinien zostać usunięty.
	 */
	ULONG rv = InterlockedIncrement(&m_ref);
	if (rv == 0) delete this;
	return rv;
}

HRESULT fabryka2::LockServer(BOOL blocked)
{
	if (blocked) numberOfUsedInstances++;
	else numberOfUsedInstances--;

	return S_OK;
}

HRESULT fabryka2::CreateInstance(IUnknown * outerInterface, REFIID InterfaceIdentifier, void ** InterfacePointer)
{
	if (InterfacePointer == NULL) return E_POINTER;
	*InterfacePointer = NULL;
	if (InterfaceIdentifier != IID_IUnknown && InterfaceIdentifier != IID_IKlasa2) return E_NOINTERFACE;

	Klasa2 *obj = new (std::nothrow) Klasa2();
	if (obj == NULL)
		return E_OUTOFMEMORY;

	HRESULT rv = obj->QueryInterface(InterfaceIdentifier, InterfacePointer);
	if (FAILED(rv))
	{
		delete obj;
		*InterfacePointer = NULL;
	};
	return rv;

}
