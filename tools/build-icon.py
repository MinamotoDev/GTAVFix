"""Render the project's vector icon. Requires Pillow only when regenerating assets."""
from pathlib import Path
from PIL import Image, ImageDraw
root=Path(__file__).resolve().parents[1]/'src/GTAVFix.App/Assets'
root.mkdir(parents=True,exist_ok=True)
svg='''<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <rect x="32" y="32" width="448" height="448" rx="100" fill="#1b2429"/>
  <path d="M94 360 184 128H238L328 360H270L252 308H168L150 360ZM185 260H235L210 186Z" fill="#f5f8f7" fill-rule="evenodd"/>
  <rect x="296" y="288" width="160" height="160" rx="48" fill="#71dfb0"/>
  <path d="M376 326V410M334 368H418" fill="none" stroke="#1b2429" stroke-width="24" stroke-linecap="round"/>
</svg>'''
(root/'App.svg').write_text(svg+'\n',encoding='utf-8')
scale=4
im=Image.new('RGBA',(512*scale,512*scale))
d=ImageDraw.Draw(im)
def coords(points):return [(int(x*scale),int(y*scale)) for x,y in points]
d.rounded_rectangle((32*scale,32*scale,480*scale,480*scale),radius=100*scale,fill='#1b2429')
d.polygon(coords([(94,360),(184,128),(238,128),(328,360),(270,360),(252,308),(168,308),(150,360)]),fill='#f5f8f7')
d.polygon(coords([(185,260),(235,260),(210,186)]),fill='#1b2429')
d.rounded_rectangle((296*scale,288*scale,456*scale,448*scale),radius=48*scale,fill='#71dfb0')
for box in [(364,314,388,422),(322,356,430,380)]:
 d.rounded_rectangle(tuple(v*scale for v in box),radius=12*scale,fill='#1b2429')
im=im.resize((512,512),Image.Resampling.LANCZOS)
im.save(root/'App.png')
im.save(root/'App.ico',format='ICO',sizes=[(n,n) for n in [16,20,24,32,40,48,64,96,128,256]])
print(root)
