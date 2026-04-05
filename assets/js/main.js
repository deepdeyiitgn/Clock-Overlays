// ==========================================
// 1. Mobile Navigation Toggle
// ==========================================
const toggle = document.querySelector('.nav-toggle');
const navLinks = document.querySelector('.nav-links');

if (toggle && navLinks) {
  toggle.addEventListener('click', () => {
    navLinks.classList.toggle('open');
  });
}

// ==========================================
// 2. Dynamic Copyright Year
// ==========================================
const yearTargets = document.querySelectorAll('.js-year');
if (yearTargets.length) {
  const year = new Date().getFullYear();
  yearTargets.forEach((target) => {
    target.textContent = year;
  });
}

// ==========================================
// 3. Premium 3D Mouse Tracking Tilt Effect
// ==========================================
// Target all main screenshot frames across the site
const tiltElements = document.querySelectorAll('.screenshot-frame, .screenshot-frame-prep, .uptodown-screenshot');

tiltElements.forEach(el => {
  // Setup initial CSS properties for smooth rendering
  el.style.transformStyle = 'preserve-3d';
  el.style.willChange = 'transform';
  
  el.addEventListener('mousemove', (e) => {
    const rect = el.getBoundingClientRect();
    
    // Get mouse coordinates relative to the element
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    
    // Find the exact center of the element
    const centerX = rect.width / 2;
    const centerY = rect.height / 2;
    
    // Calculate rotation limits (5 degrees max for a subtle, premium feel)
    // The negative sign on tiltX makes it tilt towards the mouse
    const tiltX = ((y - centerY) / centerY) * -5; 
    const tiltY = ((x - centerX) / centerX) * 5;  
    
    // Apply the 3D rotation and a slight pop-out scale
    el.style.transform = `perspective(1200px) rotateX(${tiltX}deg) rotateY(${tiltY}deg) scale(1.02)`;
  });

  el.addEventListener('mouseenter', () => {
    // Fast transition for immediate tracking when mouse enters
    el.style.transition = 'transform 0.1s ease-out';
  });

  el.addEventListener('mouseleave', () => {
    // Smooth, slow reset when mouse leaves
    el.style.transition = 'transform 0.6s cubic-bezier(0.23, 1, 0.32, 1)';
    el.style.transform = 'perspective(1200px) rotateX(0deg) rotateY(0deg) scale(1)';
  });
});
