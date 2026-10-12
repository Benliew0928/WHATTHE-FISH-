// Pack committed full-quality masters into the three compact mobile atlases.
const {build}=require('./Sports-IconAtlas.cjs');
Promise.all(['basketball','football','props'].map(build)).catch(e=>{console.error(e);process.exitCode=1;});
