// Gestion des clics en dehors du menu utilisateur
let clickOutsideHandler = null;

export function setupClickOutside(menuElement, dotNetHelper) {
  if (clickOutsideHandler) {
    removeClickOutside();
  }

  const handleClickOutside = function (event) {
    if (menuElement && !menuElement.contains(event.target)) {
      dotNetHelper.invokeMethodAsync("CloseUserMenu");
    }
  };

  clickOutsideHandler = handleClickOutside;

  setTimeout(() => {
    document.addEventListener("click", handleClickOutside);
  }, 0);
}

export function removeClickOutside() {
  if (clickOutsideHandler) {
    document.removeEventListener("click", clickOutsideHandler);
    clickOutsideHandler = null;
  }
}
