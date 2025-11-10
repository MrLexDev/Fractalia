mergeInto(LibraryManager.library, {
    DownloadImage: function(base64Ptr, fileNamePtr) {
        var base64 = UTF8ToString(base64Ptr);
        var fileName = UTF8ToString(fileNamePtr);
        try {
            var byteCharacters = atob(base64);
            var byteLength = byteCharacters.length;
            var byteArray = new Uint8Array(byteLength);
            for (var i = 0; i < byteLength; ++i) {
                byteArray[i] = byteCharacters.charCodeAt(i);
            }

            var blob = new Blob([byteArray], { type: 'image/png' });
            var url = URL.createObjectURL(blob);
            var anchor = document.createElement('a');
            anchor.href = url;
            anchor.download = fileName || 'fractalia_screenshot.png';
            document.body.appendChild(anchor);
            anchor.click();
            document.body.removeChild(anchor);
            setTimeout(function() { URL.revokeObjectURL(url); }, 1000);
        } catch (err) {
            console.error('Failed to trigger screenshot download', err);
        }
    }
});
