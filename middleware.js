export default function middleware(request) {
  const url = new URL(request.url);
  const pathname = url.pathname;

  // Rule 1: Agar path mein "/assets" hai, toh usko chhod do (Bypass for images/CSS/JS)
  if (pathname.includes('/assets/')) {
    return; 
  }

  // Rule 2: Agar kisi root file mein extension hai (jaise favicon.ico) par woh .html nahi hai, toh bypass karo
  if (pathname.includes('.') && !pathname.toLowerCase().endsWith('.html')) {
    return; 
  }

  // Rule 3: Baaki bache huye saare HTML pages ke liye
  if (pathname !== pathname.toLowerCase()) {
    url.pathname = pathname.toLowerCase();
    
    // SEO ke liye 308 (Modern 301 Moved Permanently) use kar rahe hain
    // Isse SEO ranking direct naye lowercase URL par transfer ho jayegi
    return Response.redirect(url, 307); 
  }
}
