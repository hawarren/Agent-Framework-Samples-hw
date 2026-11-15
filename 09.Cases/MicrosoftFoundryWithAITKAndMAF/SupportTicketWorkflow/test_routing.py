import operator
import re
import unittest
from decimal import Decimal
from pathlib import Path


class RoutingContractTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        guide = Path(__file__).with_name("README.md").read_text(encoding="utf-8")
        cls.confidence = re.search(
            r"Local\.TriageOutputJson\.confidence\s*(>=|<=|>|<|=)\s*([0-9.]+)",
            guide,
        )
        cls.category = re.search(
            r'Local\.TriageOutputJson\.category\s*=\s*"([^"]+)"', guide
        )
        if not cls.confidence or not cls.category:
            raise AssertionError("Portal routing expressions not found in the guide")

    def route(self, category, confidence):
        comparisons = {
            ">": operator.gt,
            ">=": operator.ge,
            "<": operator.lt,
            "<=": operator.le,
            "=": operator.eq,
        }
        comparison, threshold = self.confidence.groups()
        if not comparisons[comparison](Decimal(confidence), Decimal(threshold)):
            return "request details"
        if category == self.category.group(1):
            return "escalate"
        return "resolve"

    def test_routes_and_boundary(self):
        for category, confidence, expected in (
            ("Billing", "0.59", "request details"),
            ("Billing", "0.60", "request details"),
            ("Billing", "0.61", "escalate"),
            ("Technical", "0.60", "request details"),
            ("Technical", "0.61", "resolve"),
            ("General", "0.90", "resolve"),
        ):
            with self.subTest(category=category, confidence=confidence):
                self.assertEqual(self.route(category, confidence), expected)


if __name__ == "__main__":
    unittest.main()
