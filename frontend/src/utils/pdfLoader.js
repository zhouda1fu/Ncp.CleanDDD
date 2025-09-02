import jsPDF from "jspdf";
import html2canvas from "html2canvas";

import { ElMessage } from "element-plus";
// 纸张宽高格式
const pageFormats = {
  a0: [2383.94, 3370.39],
  a1: [1683.78, 2383.94],
  a2: [1190.55, 1683.78],
  a3: [841.89, 1190.55],
  a4: [595.28, 841.89],
  a5: [419.53, 595.28],
  a6: [297.64, 419.53],
  a7: [209.76, 297.64],
  a8: [147.4, 209.76],
  a9: [104.88, 147.4],
  a10: [73.7, 104.88],
  b0: [2834.65, 4008.19],
  b1: [2004.09, 2834.65],
  b2: [1417.32, 2004.09],
  b3: [1000.63, 1417.32],
  b4: [708.66, 1000.63],
  b5: [498.9, 708.66],
  b6: [354.33, 498.9],
  b7: [249.45, 354.33],
  b8: [175.75, 249.45],
  b9: [124.72, 175.75],
  b10: [87.87, 124.72],
  c0: [2599.37, 3676.54],
  c1: [1836.85, 2599.37],
  c2: [1298.27, 1836.85],
  c3: [918.43, 1298.27],
  c4: [649.13, 918.43],
  c5: [459.21, 649.13],
  c6: [323.15, 459.21],
  c7: [229.61, 323.15],
  c8: [161.57, 229.61],
  c9: [113.39, 161.57],
  c10: [79.37, 113.39],
  dl: [311.81, 623.62],
  letter: [612, 792],
  "government-letter": [576, 756],
  legal: [612, 1008],
  "junior-legal": [576, 360],
  ledger: [1224, 792],
  tabloid: [792, 1224],
  "credit-card": [153, 243],
};
/**
 * 生成pdf(处理多页pdf截断问题)
 * @param {Object} param
 * @param {HTMLElement} param.element - 需要转换的dom根节点
 * @param {number} [param.contentWidth] - 一页pdf的内容宽度，A4-[0-595],具体参考pageFormats对象的值
 * @param {number} [param.contentHeight] - 一页pdf的内容高度，A4-[0-842]，具体参考pageFormats对象的值
 * @param {number} [param.format='a4'] - 纸张格式，默认a4纸张
 * @param {string} [param.outputType='save'] - 生成pdf的数据类型，添加了'file'类型，默认save,就是生成pdf下载下来，其他支持的类型见http://raw.githack.com/MrRio/jsPDF/master/docs/jsPDF.html#output
 * @param {number} [param.scale=window.devicePixelRatio * 2] - 清晰度控制,canvas放大倍数,默认像素比*2
 * @param {string} [param.direction='p'] - 纸张方向，l横向，p竖向，默认A4纸张
 * @param {string} [param.fileName='document.pdf'] - pdf文件名，当outputType='file'时候，需要加上.pdf后缀
 * @param {number} param.baseX - pdf页内容距页面左边的高度，默认居中显示，为(A4宽度 - contentWidth) / 2)
 * @param {number} param.baseY - pdf页内容距页面上边的高度，默认 15px
 * @param {HTMLElement} param.header - 页眉dom元素
 * @param {HTMLElement} param.footer - 页脚dom元素
 * @param {HTMLElement} param.headerFirst - 第一页的页眉dom元素（如果需要指定第一页不同页眉时候再传这个,高度可以和其他页眉不一样)
 * @param {HTMLElement} param.footerFirst - 第一页页脚dom元素
 * @param {string} [param.groupName='pdf-group'] 类名- 给dom添加组标识的名字，分组代表要进行分页判断，当前组大于一页则新起一页，否则接着上一页
 * @param {string} [param.itemName='pdf-group-item'] 类名- 给dom添加元素标识的名字,设置了itemName代表此元素内容小于一页并且不希望被拆分，子元素也不需要遍历，即手动指定深度终点，优化性能
 * @param {string} [param.editorName='pdf-editor'] 类名- 富文本标识类
 * @param {string} [param.tableSplitName='el-table__row'] 类名- 表格组件内部的深度节点
 * @param {string} [param.offsetItemName='el-table__row'] - 用以处理像表格这样有边框的但是切页导致边框上页有但是下页没有情况，通过指定类名，会自动计算偏移一像素
 * @param {string} [param.splitName='pdf-split-page'] 类名- 强制分页，某些情况下可能想不同元素单独起一页，可以设置这个类名
 * @param {string} [param.isPageMessage=false] - 是否显示当前生成页数状态
 * @param {string} [param.isTransformBaseY=false] - 是否将baseY按照比例缩小(一般固定A4页边距时候可以用上)
 * @param {Array} [param.potionGroup=[]] - 需要计算位置的元素属性，格式是 data-position='xxx'，需要同时在节点上加上param.itemName，如<p data-position='p-position' class='pdf-group-item'></p>
 * @returns {Promise} 根据outputType返回不同的数据类型,是一个对象，包含pdf结果及需要计算的元素位置信息
 */

export class PdfLoader {
  constructor(element, param = {}) {
    if (!(element instanceof HTMLElement)) {
      throw new TypeError("element节点请传入dom节点");
    }
    console.log("[ param ] >", param);
    this.element = element;
    this.outputType = param.outputType || "save";
    this.fileName = param.fileName || "导出的pdf文件";
    this.scale = param.scale;
    this.baseX = param.baseX;
    this.baseY = param.baseY == null ? 15 : param.baseY;
    this.isTransformBaseY = param.isTransformBaseY || false;

    this.header = param.header;
    this.footer = param.footer;
    this.headerFirst = param.headerFirst;
    this.footerFirst = param.footerFirst;

    this.isPageMessage = param.isPageMessage;
    this.potionGroup = param.potionGroup || [];

    this.groupName = param.groupName || "pdf-group";
    this.itemName = param.itemName || "pdf-group-item";
    this.editorName = param.editorName || "pdf-editor";
    this.tableSplitName = param.tableSplitName || "el-table__row";
    this.offsetItemName = param.offsetItemName || "el-table__row";

    this.splitName = param.splitName || "pdf-split-page";
    this.direction = param.direction || "p"; // 默认竖向,l横向
    this.format = param.format || "a4"; // 默认a4纸尺寸

    this.pageWidth = pageFormats[this.format][0]; // 纸张的宽度
    this.pageHeight = pageFormats[this.format][1]; // 纸张的高度

    if (this.direction === "l") {
      // 如果是横向，交换宽高参数
      [this.pageHeight, this.pageWidth] = [this.pageWidth, this.pageHeight];
    }

    // 内容宽度，不指定默认留白各20
    this.contentWidth = param.contentWidth || this.pageWidth - 40;

    // 页眉页脚高度
    this.pdfFirstHeaderH = 0;
    this.pdfFirstFooterH = 0;
    this.pdfFooterHeight = 0;
    this.pdfHeaderHeight = 0;

    this.pdf = null;
    this.rate = 1; // 缩放比率
    this.pages = []; // 当前分页数据
    this.elementTop = 0; // 根元素距离可视区域高度
    this.preNode = null; // 记录遍历时候上一个节点
    this.positionData = {}; // 计算位置保存的对象
  }

  /**
   * 将元素转化为canvas元素
   * @param {HTMLElement} element - 当前要转换的元素
   * @param {width} width - 内容宽度
   * @returns
   */
  async toCanvas(element, width) {
    // canvas元素
    let canvas = await html2canvas(element, {
      allowTaint: true, // 允许渲染跨域图片
      scale: this.scale || window.devicePixelRatio * 2, // 增加清晰度
      useCORS: true, // 允许跨域
    });
    // 获取canvas转化后的宽度
    const canvasWidth = canvas.width;
    // 获取canvas转化后的高度
    const canvasHeight = canvas.height;
    // 高度转化为PDF的高度
    const height = (width / canvasWidth) * canvasHeight;
    // 转化成图片Data
    const canvasData = canvas.toDataURL("image/jpeg", 1.0);
    // 释放canvas
    // canvas = null;
    //console.log(canvasData)
    return { width, height, data: canvasData };
  }

  /**
   * 生成pdf方法，外面调用这个方法
   * @returns {Promise} 返回一个promise
   */
  getPdf() {
    // 滚动置顶,防止顶部空白
    window.pageYoffset = 0;
    document.documentElement.scrollTop = 0;
    document.body.scrollTop = 0;
    return new Promise(async (resolve, reject) => {
      // jsPDF实例
      const pdf = new jsPDF({
        unit: "pt",
        format: this.format,
        orientation: this.direction,
      });

      this.pdf = pdf;
      let pdfFooterHeight = 0;
      let pdfHeaderHeight = 0;
      let pdfFirstHeaderH = 0; // 第一页的页眉
      let pdfFirstFooterH = 0; // 第一页的页脚

      // 距离PDF左边的距离，/ 2 表示居中 ,,预留空间给左边,  右边，也就是左右页边距
      let baseX = (this.pageWidth - this.contentWidth) / 2;

      // 距离PDF 页眉和页脚的间距， 留白留空
      let baseY = this.baseY;

      // 元素在网页页面的宽度
      const elementWidth = this.element.scrollWidth;
      console.log("elementWidth", elementWidth);

      // PDF内容宽度 和 在HTML中宽度 的比， 用于将 元素在网页的高度 转化为 PDF内容内的高度， 将 元素距离网页顶部的高度  转化为 距离Canvas顶部的高度
      const rate = this.contentWidth / elementWidth;
      this.rate = rate;

      if (this.isTransformBaseY) {
        this.baseY = baseY = baseY * rate;
      }

      // 页脚元素 经过转换后在PDF页面的高度
      if (this.footer) {
        pdfFooterHeight = (await this.toCanvas(this.footer, this.pageWidth)).height;
        this.pdfFooterHeight = pdfFooterHeight;
        pdfFirstFooterH = pdfFooterHeight; // 默认第一页和其他页页眉页脚一样
      }

      // 页眉元素 经过转换后在PDF的高度
      if (this.header) {
        pdfHeaderHeight = (await this.toCanvas(this.header, this.pageWidth)).height;
        this.pdfHeaderHeight = pdfHeaderHeight;
        pdfFirstHeaderH = pdfHeaderHeight;
      }

      // 第一页-- 页眉元素 经过转换后在PDF的高度
      if (this.headerFirst) {
        pdfFirstHeaderH = (await this.toCanvas(this.headerFirst, this.pageWidth)).height;
        this.pdfFirstHeaderH = pdfFirstHeaderH;
      }

      // 第一页-- 页脚元素 经过转换后在PDF的高度
      if (this.footerFirst) {
        pdfFirstFooterH = (await this.toCanvas(this.footerFirst, this.pageWidth)).height;
        this.pdfFirstFooterH = pdfFirstFooterH;
      }

      // 除去页头、页眉、还有内容与两者之间的间距后 每页内容的实际高度
      const originalPageHeight = this.pageHeight - pdfFooterHeight - pdfHeaderHeight - 2 * baseY;
      this.originalPageHeight = originalPageHeight;

      // 第一页的  除去页头、页眉、还有内容与两者之间的间距后 每页内容的实际高度
      const firstOriginalPageHeight = this.pageHeight - pdfFirstFooterH - pdfFirstHeaderH - 2 * baseY;
      this.firstOriginalPageHeight = firstOriginalPageHeight;

      // 每一页的分页坐标， PDF高度， 初始值为根元素距离顶部的距离
      this.elementTop = this.getElementTop(this.element);
      // this.pages = [0]; // 要从0开始
      this.pages = [
        {
          position: 0,
          offset: 0,
        },
      ]; // 要从0开始

      // 深度遍历节点的方法
      this.traversingNodes(this.element.childNodes);

      const pages = this.pages;
      console.log("%c [ pages ]-143", "font-size:13px; background:pink; color:#bf2c9f;", pages);

      // 一页的高度， 转换宽度为一页元素的宽度
      const { width, height, data } = await this.toCanvas(this.element, this.contentWidth);
      // 可能会存在遍历到底部元素为深度节点，可能存在最后一页位置未截取到的情况
      // if (pages[pages.length - 1] + originalPageHeight < height) {
      //   pages.push(pages[pages.length - 1] + originalPageHeight);
      // }

      // 根据分页位置 开始分页生成pdf
      for (let i = 0; i < pages.length; ++i) {
        if (this.isPageMessage) {
          ElMessage.success(`共${pages.length}页， 生成第${i + 1}页`);
        }

        // 页眉高度
        let pdfHeaderH = pdfHeaderHeight;
        // 页脚高度
        let pdfFooterH = pdfFooterHeight;

        // 如果是第一页，以第一页页眉高度为准
        if (i === 0) {
          pdfHeaderH = pdfFirstHeaderH;
          pdfFooterH = pdfFirstFooterH;
        }

        // 根据分页位置新增图片
        this.addImage(baseX, baseY + pdfHeaderH - pages[i].position, pdf, data, width, height);

        // 将 内容 与 页眉之间留空留白的部分进行遮白处理
        this.addBlank(0, pdfHeaderH, this.pageWidth, baseY, pdf);

        // 将 内容 与 页脚之间留空留白的部分进行遮白处理
        this.addBlank(0, this.pageHeight - baseY - pdfFooterH, this.pageWidth, baseY, pdf);

        // 对于除最后一页外，对 内容 的多余部分进行遮白处理
        if (i < pages.length - 1) {
          // 获取当前页面需要的内容部分高度,并偏移offset
          const imageHeight = pages[i + 1].position - pages[i].position + pages[i].offset;
          // 对多余的内容部分进行遮白
          this.addBlank(0, baseY + imageHeight + pdfHeaderH, this.pageWidth, this.pageHeight - imageHeight, pdf);
        }

        // 添加页眉
        await this.addHeader(i + 1, this.header, pdf, this.pageWidth);

        // 添加页脚
        await this.addFooter(pages.length, i + 1, this.footer, pdf, this.pageWidth);

        // 若不是最后一页，则分页
        if (i !== pages.length - 1) {
          // 增加分页
          pdf.addPage();
        }
      }

      try {
        const result = await this.getPdfByType(pdf);
        resolve({
          positionData: this.positionData,
          pdfResult: result,
        });
      } catch (error) {
        reject("生成pdf出错", error);
      }
    });
  }

  // 根据类型获取pdf
  getPdfByType(pdf) {
    let result = null;
    switch (this.outputType) {
      case "file":
        result = new File([pdf.output("blob")], this.fileName, {
          type: "application/pdf",
          lastModified: Date.now(),
        });
        break;
      case "save":
        result = pdf.save(this.fileName);
        break;
      default:
        result = pdf.output(this.outputType);
    }
    return result;
  }

  /**
   * 遍历正常的元素节点
   * @param {HTMLElement} nodes - 当前要遍历的节点数组
   * @returns
   */
  traversingNodes(nodes) {
    for (let i = 0; i < nodes.length; ++i) {
      const one = nodes[i];
      // 需要判断跨页且内部存在跨页的元素,以分组类名区分
      const isGround = one.classList && one.classList.contains(this.groupName);
      // 小模块，并且内部不需要遍历了，作为深度终点
      const isItem = one.classList && one.classList.contains(this.itemName);
      // 强制分页的标记点
      const isSplit = one.classList && one.classList.contains(this.splitName);

      // 图片元素不需要继续深入，作为深度终点
      const isIMG = one.tagName === "IMG";
      // table的每一行元素也是深度终点
      const isTableCol = one.classList && one.classList.contains(this.tableSplitName);

      // 要进行边框偏移的
      const isOffsetItem = one.classList && one.classList.contains(this.offsetItemName);

      // 特殊的富文本元素
      const isEditor = one.classList && one.classList.contains(this.editorName);
      // 对需要处理分页的元素，计算是否跨界，若跨界，则直接将顶部位置作为分页位置，进行分页，且子元素不需要再进行判断
      let { offsetHeight } = one;
      // 计算出最终高度，要减去根元素顶部高度
      let offsetTop = this.getElementTop(one) - this.elementTop;

      // dom转换后距离顶部的高度
      // 转换成canvas高度
      const top = this.rate * offsetTop;

      if (isSplit) {
        // this.pages.push(top);
        this.pages.push({
          position: top,
          offset: 0, // 偏移
        });
        // 执行深度遍历操作
        this.traversingNodes(one.childNodes);
      }
      // 对于需要进行分页且内部存在需要分页（即不属于深度终点）的元素进行处理
      else if (isGround) {
        // 执行位置更新操作
        this.updatePos(this.rate * offsetHeight, top, one);
        // 执行深度遍历操作
        this.traversingNodes(one.childNodes);
      }
      // 对于深度终点元素进行处理
      else if (isTableCol || isIMG || isItem) {
        // dom高度转换成生成pdf的实际高度
        // 代码不考虑dom定位、边距、边框等因素，需在dom里自行考虑，如将box-sizing设置为border-box
        let boo = this.updatePos(this.rate * offsetHeight, top, one);

        if (isTableCol && boo) {
          // 表格分页截断部分添加一个单独类名，用于外部可以设置边框样式
          one.classList.add("pdf-table-split-tr");
          this.preNode && this.preNode.classList.add("pdf-table-split-tr-pre");
        }
        // 表格行，记录上一个节点
        if (isTableCol) {
          this.preNode = one;
        }

        // 设置偏移量
        if ((isOffsetItem || isTableCol) && boo) {
          this.pages[this.pages.length - 2].offset += 1.5;
        }

        // 计算位置
        if (isItem) {
          this.getItemPosition(one, top, offsetTop);
        }
      } else if (isEditor) {
        // 执行位置更新操作
        this.updatePos(this.rate * offsetHeight, top, one);
        // 遍历富文本节点
        this.traversingEditor(one.childNodes);
      }
      // 对于普通元素，则判断是否高度超过分页值，并且深入
      else {
        // 执行位置更新操作
        this.updateNormalElPos(top);
        // this.updatePos(this.rate * offsetHeight, top, one);
        // 遍历子节点
        this.traversingNodes(one.childNodes);
      }
    }
    return;
  }

  /**
   * 对于富文本元素，观察所得段落之间都是以<p> / <img> 元素相隔，因此不需要进行深度遍历
   * (这个可以根据自己的富文本结构进行改动优化)
   * @param {HTMLElement} nodes - 当前要遍历的节点数组
   * @returns
   */
  traversingEditor(nodes) {
    // 遍历子节点
    for (let i = 0; i < nodes.length; ++i) {
      const one = nodes[i];
      let { offsetHeight } = one;
      let offsetTop = this.getElementTop(one) - this.elementTop;
      const top = this.rate * offsetTop;
      this.updatePos(this.rate * offsetHeight, top, one);
    }
  }

  /**
   *
   * 获取元素在页面位置
   * @param {HTMLElement} node 当前节点
   * @param {number} offsetTop 当前节点距离顶部高度，已经按比率转换的高度
   * @returns
   */
  getItemPosition(node, top, offsetTop) {
    if (node.dataset.position) {
      // 位置信息对象 (当前元素快左上角点位置)
      const potionObj = {
        page: 0, // 当前页
        offsetX: 0, // 距离当前页左测距离
        offsetY: 0, // 距离当前页顶部距离
        width: 0, // 当前元素块宽
        height: 0, // 当前元素块高
        offsetB: 0, // 距离底部页距离
        offsetBP: 0, // 距离底部页脚距离
      };

      // 页眉高度
      let pdfHeaderH = this.pdfHeaderHeight;
      let pdfFooterH = this.pdfFooterHeight;

      // 如果是第一页，以第一页页眉高度为准
      if (this.pages.length === 1 && this.pdfFirstHeaderH) {
        pdfHeaderH = this.pdfFirstHeaderH;
        pdfFooterH = this.pdfFirstFooterH;
      }

      // 计算上一页分页点的高度
      const pageH = this.pages.length > 0 ? this.pages[this.pages.length - 1].position : 0;

      // y 计算距离当前页顶部高度（当前元素高度 - 上一个页点高度 - 页眉 - 留白）
      let offsetY = top - pageH + pdfHeaderH;
      potionObj.offsetY = offsetY / this.rate + this.baseY;

      // 底部距离，页高度减去顶部距离
      potionObj.offsetBP = (this.pageHeight - offsetY - pdfFooterH) / this.rate;
      potionObj.offsetB = (this.pageHeight - offsetY) / this.rate;

      // x 坐标以当前导出容器定位为准
      potionObj.offsetX = node.offsetLeft;

      potionObj.width = node.offsetWidth;
      potionObj.height = node.offsetHeight;

      // 注意这里保存是以0开始，也就是第一页返回的是0
      potionObj.page = this.pages.length - 1;

      // 保存
      this.positionData[node.dataset.position] = potionObj;
    }
  }

  /**
   *  可能跨页元素位置更新的方法
   *  需要考虑分页元素，则需要考虑两种情况
   *  1. 普通达顶情况，如上
   *  2. 当前距离顶部高度加上元素自身高度 大于 整页高度，则需要载入一个分页点
   * @param {Number} eleHeight - 当前元素在pdf中的高度（经过比例转换的）
   * @param {Number} top - 当前元素在pdf中距离顶部可视区域高度（经过比例转换)
   * @return
   */
  updatePos(eleHeight, top) {
    const pageH = this.pages.length > 0 ? this.pages[this.pages.length - 1].position : 0;
    let originalPageHeight = this.originalPageHeight;

    // 如果是第一页，以第一页为准
    if (this.pages.length === 1) {
      originalPageHeight = this.firstOriginalPageHeight;
    }

    // 如果高度已经超过当前页，则证明可以分页了
    if (top - pageH >= originalPageHeight) {
      // this.pages.push(pageH + originalPageHeight);
      this.pages.push({
        position: pageH + originalPageHeight,
        offset: 0, // 偏移
      });
      return true;
    }
    // 若 距离当前页顶部的高度 加上元素自身的高度 大于 一页内容的高度, 则证明元素跨页，将当前高度作为分页位置
    // top！=pageH这个条件是防止多个元素嵌套情况下，他们top是一样的
    else if (top + eleHeight - pageH > originalPageHeight && top != pageH) {
      // this.pages.push(top);
      this.pages.push({
        position: top,
        offset: 0, // 偏移
      });
      return true;
    }
  }

  /**
   *  普通元素更新位置
   *  普通元素只需要考虑到是否到达了分页点，即当前距离顶部高度 - 上一个分页点的高度 大于 正常一页的高度，则需要载入分页点
   *  这种判断，有可能出现截断，但是机率比较小,可以通过设置itemName进一步减少这种情况出现
   * @param {Number} top - 当前元素在pdf中距离顶部可视区域高度（经过比例转换)
   * @returns
   */
  updateNormalElPos(top) {
    const pageH = this.pages.length > 0 ? this.pages[this.pages.length - 1].position : 0;
    let originalPageHeight = this.originalPageHeight;

    // 如果是第一页，以第一页为准
    if (this.pages.length === 1) {
      originalPageHeight = this.firstOriginalPageHeight;
    }

    if (top - pageH >= originalPageHeight) {
      // this.pages.push(pageH + originalPageHeight);
      this.pages.push({
        position: pageH + originalPageHeight,
        offset: 0, // 偏移
      });
      return true;
    }
  }
  /**
   * 获取元素距离网页顶部的距离
   * 通过遍历offsetParent获取距离顶端元素的高度值
   * @param {HTMLElement} element - 需要计算的元素
   * @returns
   */
  getElementTop(element) {
    let actualTop = element.offsetTop;
    let current = element.offsetParent;

    while (current) {
      actualTop += current.offsetTop;
      current = current.offsetParent;
    }

    return actualTop;
  }

  /**
   * 添加页眉
   * @param {HTMLElement} header -页眉元素
   * @param {Object} pdf - pdf实例
   * @param {Number} contentWidth -在pdf中占据的宽度（默认占满)
   * @returns
   */
  async addHeader(pageNo, header, pdf, contentWidth) {
    if (pageNo === 1 && this.headerFirst) {
      // 第一页
      const { height, data } = await this.toCanvas(this.headerFirst, contentWidth);
      pdf.addImage(data, "JPEG", 0, 0, contentWidth, height);
      return;
    }

    if (!header || !(header instanceof HTMLElement)) {
      return;
    }

    if (!this.__header) {
      // 其他页 页头都是一样的，不需要每次都生成
      this.__header = await this.toCanvas(header, contentWidth);
    }

    //  每页都从 0 0 开始？
    // addImage(data,format,x,y,w,h)
    pdf.addImage(this.__header.data, "JPEG", 0, 0, contentWidth, this.__header.height);
  }

  /**
   * 添加页脚
   * @param {Number} pageSize -总页数
   * @param {Number} pageNo -当前第几页
   * @param {HTMLElement} footer -页脚元素
   * @param {Object} pdf - pdf实例
   * @param {Number} contentWidth - 在pdf中占据的宽度（默认占满)
   * @returns
   */
  async addFooter(pageSize, pageNo, footer, pdf, contentWidth) {
    if (pageNo === 1 && this.footerFirst) {
      footer = this.footerFirst;
    }

    if (!footer || !(footer instanceof HTMLElement)) {
      return;
    }

    // 页码元素，类名这里写死了
    let pageNoDom = footer.querySelector(".pdf-footer-page");
    let pageSizeDom = footer.querySelector(".pdf-footer-page-count");
    if (pageNoDom) {
      pageNoDom.innerText = pageNo;
    }
    if (pageSizeDom) {
      pageSizeDom.innerText = pageSize;
    }

    if (pageNo === 1 && this.footerFirst) {
      // 第一页
      const { height, data } = await this.toCanvas(this.footerFirst, contentWidth);
      pdf.addImage(data, "JPEG", 0, this.pageHeight - height, contentWidth, height);
      return;
    }

    // 如果设置了页码的才需要每次重新生成cavan
    if (pageNoDom || !this.__footer) {
      this.__footer = await this.toCanvas(footer, contentWidth);
    }

    // 高度位置计算:当前a4高度 - 页脚在pdf中的高度
    pdf.addImage(
      this.__footer.data,
      "JPEG",
      0,
      this.pageHeight - this.__footer.height,
      contentWidth,
      this.__footer.height
    );
  }

  // 截取图片
  addImage(_x, _y, pdf, data, width, height) {
    pdf.addImage(data, "JPEG", _x, _y, width, height);
  }

  /**
   * 添加空白遮挡
   * @param {Number} x - x 与页面左边缘的坐标（以 PDF 文档开始时声明的单位）
   * @param {Number} y - y 与页面上边缘的坐标（以 PDF 文档开始时声明的单位）
   * @param {Number} width - 填充宽度
   * @param {Number} height -填充高度
   * @param {Object} pdf - pdf实例
   * @returns
   */
  addBlank(x, y, width, height, pdf) {
    pdf.setFillColor(255, 255, 255);
    // rect(x, y, w, h, style) ->'F'填充方式，默认是描边方式
    pdf.rect(x, y, Math.ceil(width), Math.ceil(height), "F");
  }
}
