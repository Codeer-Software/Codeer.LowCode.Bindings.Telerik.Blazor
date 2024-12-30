function waitForAnimationFrame() {
  return new Promise(resolve => {
    requestAnimationFrame(resolve);
  });
}

export async function waitForTelerikBlazor() {
  while (typeof window.TelerikBlazor == "undefined") {
    await waitForAnimationFrame();
  }
}