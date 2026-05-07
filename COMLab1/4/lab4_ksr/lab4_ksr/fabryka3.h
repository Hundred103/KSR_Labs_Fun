#include<windows.h>

class KlasaFactory: public IClassFactory {
public:	
	KlasaFactory();
	~KlasaFactory();
	virtual ULONG STDMETHODCALLTYPE AddRef();
	virtual ULONG STDMETHODCALLTYPE Release();
	virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID id, void **rv);
	virtual HRESULT STDMETHODCALLTYPE LockServer(BOOL v);
	virtual HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown *outer, REFIID id, void **rv);

private:	

	/*
	Tutaj zdeklaruj zmienne:
		- licznik blokad,
		- licznik referencji.
	 */
	// done
	volatile ULONG m_lock;
	volatile ULONG m_ref;

	};
