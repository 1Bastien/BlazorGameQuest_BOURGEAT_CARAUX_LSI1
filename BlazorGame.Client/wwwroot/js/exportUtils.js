// Fonction pour télécharger un fichier depuis une chaîne base64
window.downloadFile = function(filename, base64Content) {
    // Ajouter le BOM UTF-8 pour une meilleure compatibilité avec Excel
    const bom = '\uFEFF';
    const content = atob(base64Content);
    const bytes = new Uint8Array(content.length + bom.length);
    
    // Ajouter le BOM
    for (let i = 0; i < bom.length; i++) {
        bytes[i] = bom.charCodeAt(i);
    }
    
    // Ajouter le contenu
    for (let i = 0; i < content.length; i++) {
        bytes[i + bom.length] = content.charCodeAt(i);
    }
    
    const blob = new Blob([bytes], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Libérer la mémoire
    URL.revokeObjectURL(url);
};

