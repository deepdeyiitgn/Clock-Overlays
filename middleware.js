export default function middleware(request) {
  const url = new URL(request.url);
  
  // Agar request /assets folder ki hai, ya usme extension hai (jaise .png, .jpg, .css, .js)
  // Toh usko bypass kar do (kuch mat karo), taaki images original naam se load hon.
  if (url.pathname.startsWith('/assets') || url.pathname.includes('.')) {
    return; 
  }
  
  // Baaki sab pages ke URL ko small letters (lowercase) mein convert kar do
  if (url.pathname !== url.pathname.toLowerCase()) {
    url.pathname = url.pathname.toLowerCase();
    return Response.redirect(url, 308);
  }
}
