export const phoneLayout = window.matchMedia("(max-width: 760px)");
export const previewSheetLayout = window.matchMedia("(max-width: 1200px)");

export function mobileSheet(content, title, { layout = phoneLayout, lightDismiss = false } = {}) {
  const placeholder = document.createComment(title);
  content.before(placeholder);

  const dialog = document.createElement("dialog");
  dialog.className = "mobile-sheet";
  dialog.setAttribute("aria-label", title);
  if (lightDismiss) enableLightDismiss(dialog);
  const toolbar = document.createElement("div");
  toolbar.className = "mobile-sheet-toolbar";
  const heading = document.createElement("strong");
  heading.textContent = title;
  const closeButton = document.createElement("button");
  closeButton.type = "button";
  closeButton.className = "button compact-button";
  closeButton.textContent = "Close";
  closeButton.autofocus = true;
  toolbar.append(heading, closeButton);
  dialog.append(toolbar);
  document.body.append(dialog);

  let returnFocus;
  function placeContent() {
    if (layout.matches) {
      dialog.append(content);
    } else {
      dialog.close();
      placeholder.after(content);
    }
  }

  closeButton.addEventListener("click", () => dialog.close());
  dialog.addEventListener("close", () => {
    if (!dialog.open && layout.matches) returnFocus?.()?.focus({ preventScroll: true });
  });
  layout.addEventListener("change", placeContent);
  placeContent();

  return {
    open(focusTarget) {
      if (!layout.matches || dialog.open) return;
      returnFocus = focusTarget;
      dialog.showModal();
      dialog.scrollTop = 0;
    },
    close(focusTarget = returnFocus) {
      returnFocus = focusTarget;
      dialog.close();
    }
  };
}

function enableLightDismiss(dialog) {
  if ("closedBy" in dialog) {
    dialog.closedBy = "any";
    return;
  }

  function isBackdrop(event) {
    if (event.target !== dialog) return false;
    const bounds = dialog.getBoundingClientRect();
    return event.clientX < bounds.left || event.clientX >= bounds.right
      || event.clientY < bounds.top || event.clientY >= bounds.bottom;
  }

  let pressedBackdrop = false;
  dialog.addEventListener("pointerdown", event => {
    pressedBackdrop = isBackdrop(event);
  });
  dialog.addEventListener("click", event => {
    if (pressedBackdrop && isBackdrop(event)) dialog.close();
    pressedBackdrop = false;
  });
}
