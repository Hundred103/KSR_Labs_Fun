#include<windows.h>
#include<stdio.h>
#include"klasa3.h"


extern volatile int usageCount;


Klasa3::Klasa3() {
	usageCount++;
	};


Klasa3::~Klasa3() {
	usageCount--;
	};


ULONG STDMETHODCALLTYPE Klasa3::AddRef() {
	//done
	InterlockedIncrement(&m_ref);
	return m_ref;
	};


ULONG STDMETHODCALLTYPE Klasa3::Release() {
	//done
	ULONG rv = InterlockedDecrement(&m_ref);
	if(rv == 0) delete this;
	return rv;
	};


HRESULT STDMETHODCALLTYPE Klasa3::QueryInterface(REFIID iid, void **ptr) {
	if(ptr == NULL) return E_POINTER;
	if(IsBadWritePtr(ptr, sizeof(void *))) return E_POINTER;
	*ptr = NULL;
	if(iid == IID_IUnknown) *ptr = this;
	if(iid == IID_IKlasa3) *ptr = this;
	if(*ptr != NULL) { AddRef(); return S_OK; };
	return E_NOINTERFACE;
	};

HRESULT STDMETHODCALLTYPE Klasa3::Test(const char *napis){
	//done
	printf(napis);
	return S_OK;
	};

