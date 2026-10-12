// Node.js + sharp. The common packer resolves sources from this repository.
require('./Sports-IconAtlas.cjs').build('basketball').catch(e=>{console.error(e);process.exitCode=1;});
