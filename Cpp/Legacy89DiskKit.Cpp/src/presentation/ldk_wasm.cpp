#include "legacy89diskkit_native.h"

#include <cstdlib>
#include <cstddef>

#ifdef __EMSCRIPTEN__
#include <emscripten.h>
#else
#define EMSCRIPTEN_KEEPALIVE
#endif

extern "C" {

EMSCRIPTEN_KEEPALIVE
int32_t ldk_wasm_get_abi_version() {
    return ldk_get_abi_version();
}

EMSCRIPTEN_KEEPALIVE
void* ldk_wasm_allocate(int32_t size) {
    if (size <= 0) return nullptr;
    return malloc(static_cast<size_t>(size));
}

EMSCRIPTEN_KEEPALIVE
void ldk_wasm_free(void* ptr) {
    free(ptr);
}

}

#ifndef __EMSCRIPTEN__
int main() {
    return ldk_wasm_get_abi_version() > 0 ? 0 : 1;
}
#endif
