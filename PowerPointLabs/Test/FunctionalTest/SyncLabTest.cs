﻿using System;
using System.Drawing;
using System.Windows.Forms;
using TestInterface;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Util;
using Point = System.Drawing.Point;
using System.Collections.Generic;
using System.Threading.Tasks;
using PowerPointLabs;

namespace Test.FunctionalTest
{
    [TestClass]
    public class SyncLabTest : BaseFunctionalTest
    {
        private const int OriginalSyncGroupToShapeSlideNo = 36;
        private const int ExpectedSyncGroupToShapeSlideNo = 37;
        private const int OriginalSyncShapeToGroupSlideNo = 38;
        private const int ExpectedSyncShapeToGroupSlideNo = 39;

        private const string Line = "Straight Connector 2";
        private const string RotatedArrow = "Right Arrow 5";
        private const string Group = "Group 1";
        private const string Oval = "Oval 4";
        private const string CopyFromShape = "CopyFrom";
        private const string UnrotatedRectangle = "Rectangle 3";

        protected override string GetTestingSlideName()
        {
            return "SyncLab.pptx";
        }

        [TestMethod]
        [TestCategory("FT")]
        public void FT_SyncLabTest()
        {
            var syncLab = PplFeatures.SyncLab;
            syncLab.OpenPane();

            TestSync(syncLab);
            TestErrorDialogs(syncLab);
        }

        private void TestErrorDialogs(ISyncLabController syncLab)
        {
            PpOperations.SelectSlide(OriginalSyncGroupToShapeSlideNo);

            // no selection copy
            MessageBoxUtil.ExpectMessageBoxWillPopUp(TextCollection.SyncLabErrorDialogTitle,
                "Please select one shape to copy.", syncLab.Copy, "Ok");

            // 2 item selected copy
            List<String> shapes = new List<string> { Line, RotatedArrow };
            PpOperations.SelectShapes(shapes);
            MessageBoxUtil.ExpectMessageBoxWillPopUp(TextCollection.SyncLabErrorDialogTitle,
                "Please select one shape to copy.", syncLab.Copy, "Ok");

            // group selected copy
            PpOperations.SelectShape(Group);
            MessageBoxUtil.ExpectMessageBoxWillPopUp(TextCollection.SyncLabErrorDialogTitle,
                "Please select one shape to copy.", syncLab.Copy, "Ok");

            // copy blank item for the paste error dialog test
            PpOperations.SelectShape(Line);    
            syncLab.Copy();
            syncLab.DialogClickOk();

            // no selection sync
            PpOperations.SelectSlide(ExpectedSyncShapeToGroupSlideNo);
            MessageBoxUtil.ExpectMessageBoxWillPopUp(TextCollection.SyncLabErrorDialogTitle,
                "Please select at least one item to apply.", () => syncLab.Sync(0), "Ok");
        }

        private void TestSync(ISyncLabController syncLab)
        {
            Sync(syncLab, OriginalSyncGroupToShapeSlideNo, ExpectedSyncGroupToShapeSlideNo, CopyFromShape, RotatedArrow);
            Sync(syncLab, OriginalSyncShapeToGroupSlideNo, ExpectedSyncShapeToGroupSlideNo, Line, Oval);
        }

        private void Sync(ISyncLabController syncLab, int originalSlide, int expectedSlide, string fromShape, string toShape)
        {
            PpOperations.SelectSlide(originalSlide);
            PpOperations.SelectShape(fromShape);

            syncLab.Copy();
            syncLab.DialogSelectItem(1, 0);
            syncLab.DialogClickOk();

            PpOperations.SelectShape(toShape);
            syncLab.Sync(0);

            IsSame(originalSlide, expectedSlide, toShape);
        }

        private void IsSame(int originalSlideNo, int expectedSlideNo, string shapeToCheck)
        {
            var actualSlide = PpOperations.SelectSlide(originalSlideNo);
            var actualShape = PpOperations.SelectShape(shapeToCheck)[1];
            var expectedSlide = PpOperations.SelectSlide(expectedSlideNo);
            var expectedShape = PpOperations.SelectShape(shapeToCheck)[1];
            SlideUtil.IsSameLooking(expectedSlide, actualSlide);
            SlideUtil.IsSameShape(expectedShape, actualShape);
        }
    }
}